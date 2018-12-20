using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using DirectSp.Core.Infrastructure;
using DirectSp.Core.InternalDb;
using DirectSp.Core.SpSchema;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.Core
{
    public class SpInvoker
    {
        private UserSessionManager SessionManager;
        private JwtTokenSigner _tokenSigner;
        private IDbLayer _dbLayer;

        public string ConnectionStringReadOnly { get; }
        public string ConnectionStringReadWrite { get; }
        public SpInvokerOptions Options { get; }
        public string ConnectionString { get; }
        public string Schema { get; }
        public IDspKeyValue KeyValue { get; }
        public InvokerPath InvokerPath { get; }
        internal CaptchaHandler CaptchaHandler { get; }

        public SpInvoker(SpInvokerConfig config)
        {
            InvokerPath = new InvokerPath(config.Options.WorkspaceFolderPath);

            //validate ConnectionString
            Schema = config.Schema ?? throw new Exception("Schema is not set!");
            ConnectionString = config.ConnectionString ?? throw new Exception("ConnectionString is not set!");
            Options = config.Options;
            SessionManager = new UserSessionManager(Options);
            KeyValue = config.KeyValue;
            _tokenSigner = config.TokenSigner;
            _dbLayer = config.DbLayer;
            SpException.UseCamelCase = Options.UseCamelCase;
            ConnectionStringReadOnly = new SqlConnectionStringBuilder(config.ConnectionString) { ApplicationIntent = ApplicationIntent.ReadOnly }.ToString();
            ConnectionStringReadWrite = new SqlConnectionStringBuilder(config.ConnectionString) { ApplicationIntent = ApplicationIntent.ReadWrite }.ToString();
            CaptchaHandler = new CaptchaHandler(KeyValue);
        }

        private DateTime? LastCleanTempFolderTime;

        public SpContext _AppUserContext;
        public SpContext AppUserContext
        {
            get
            {
                lock (LockObject)
                {
                    if (_AppUserContext == null)
                    {
                        RefreshApi();
                    }

                    return _AppUserContext;
                }
            }
        }

        private Dictionary<string, SpInfo> _SpInfos;
        public Dictionary<string, SpInfo> SpInfos
        {
            get
            {
                lock (LockObject)
                {
                    if (_SpInfos == null)
                    {
                        RefreshApi();
                    }

                    return _SpInfos;
                }
            }
        }

        public string AppName => AppUserContext.AppName;
        public string AppVersion { get; private set; }

        // User Request Count control
        private void VerifyUserRequestLimit(UserSession userSession)
        {
            //AppUserId does not have request limit
            if (userSession.SpContext.AuthUserId == AppUserContext.AuthUserId)
            {
                return;
            }

            //Reset ResetRequestCount
            if (userSession.RequestIntervalStartTime.AddSeconds(Options.SessionMaxRequestCycleInterval) < DateTime.Now)
            {
                userSession.ResetRequestCount();
            }

            //Reject Request
            if (userSession.RequestCount > Options.SessionMaxRequestCount)
            {
                throw new SpException("Too many request! Please try a few minutes later!", StatusCodes.Status429TooManyRequests);
            }
        }

        private readonly object LockObject = new object();

        private void RefreshApi()
        {
            lock (LockObject)
            {
                var spInfos = new Dictionary<string, SpInfo>();
                using (var sqlConnection = new SqlConnection(ConnectionStringReadOnly))
                {
                    var spList = ResourceDb.System_Api(_dbLayer, sqlConnection, out string appUserContext);
                    foreach (var item in spList)
                    {
                        spInfos.Add(item.SchemaName + "." + item.ProcedureName, item);
                    }

                    _SpInfos = spInfos;
                    _AppUserContext = new SpContext(appUserContext, "$$");
                    AppVersion = _AppUserContext.AppVersion; //don't make AppVersion property because _AppUserContext may not be initialized when there is error
                }
            }
        }

        public async Task<SpCallResult[]> Invoke(SpCall[] spCalls, SpInvokeParams spInvokeParams)
        {
            //Check DuplicateRequest if spCalls contian at least one write
            foreach (var spCall in spCalls)
            {
                var spInfo = FindSpInfo($"{Schema}.{spCall.Method}");
                if (spInfo != null && spInfo.ExtendedProps.DataAccessMode == SpDataAccessMode.Write)
                {
                    await CheckDuplicateRequest(spInvokeParams.InvokeOptions.RequestId, 3600 * 2);
                    break;
                }
            }

            var spi = new SpInvokeParamsInternal
            {
                SpInvokeParams = spInvokeParams,
                IsBatch = true
            };

            var spCallResults = new List<SpCallResult>();
            var tasks = new List<Task<SpCallResult>>();
            foreach (var spCall in spCalls)
            {
                tasks.Add(Invoke(spCall, spi));
            }

            try
            {
                await Task.WhenAll(tasks.ToArray());
            }
            catch
            {
                // catch await single exception
            }

            foreach (var item in tasks)
            {
                if (item.IsCompletedSuccessfully)
                    spCallResults.Add(item.Result);
                else
                    spCallResults.Add(new SpCallResult { { "error", SpExceptionAdapter.Convert(this, item.Exception.InnerException).SpCallError } });
            }

            return spCallResults.ToArray();
        }


        public async Task<SpCallResult> Invoke(SpCall spCall)
        {
            var spInvokeParams = new SpInvokeParams();
            return await Invoke(spCall, spInvokeParams, true);
        }

        public async Task<SpCallResult> Invoke(string method, object param)
        {
            var spInvokeParams = new SpInvokeParams();
            return await Invoke(method, param, spInvokeParams, true);
        }

        public async Task<SpCallResult> Invoke(string method, object param, SpInvokeParams spInvokeParams, bool isSystem = false)
        {
            // Create spCall
            var spCall = new SpCall
            {
                Method = method
            };

            foreach (var propInfo in param.GetType().GetProperties())
                spCall.Params.Add(propInfo.Name, propInfo.GetValue(param));

            return await Invoke(spCall, spInvokeParams, isSystem);
        }

        public async Task<SpCallResult> Invoke(SpCall spCall, SpInvokeParams spInvokeParams, bool isSystem = false)
        {
            // Check duplicate request
            var spInfo = FindSpInfo($"{Schema}.{spCall.Method}");
            if (spInfo != null && spInfo.ExtendedProps.DataAccessMode == SpDataAccessMode.Write)
                await CheckDuplicateRequest(spInvokeParams.InvokeOptions.RequestId, spInfo.ExtendedProps.CommandTimeout);

            // Call main invoke
            var spi = new SpInvokeParamsInternal { SpInvokeParams = spInvokeParams, IsSystem = isSystem };
            if (isSystem)
                spi.SpInvokeParams.AuthUserId = AppUserContext.AuthUserId;

            return await Invoke(spCall, spi);
        }

        private async Task<SpCallResult> Invoke(SpCall spCall, SpInvokeParamsInternal spi)
        {
            try
            {
                return await InvokeCore(spCall, spi);
            }
            catch (SpInvokerAppVersionException)
            {
                RefreshApi();
                return await Invoke(spCall, spi);
            }
            catch (SpException spException) //catch any read-only errors
            {
                throw spException.SpCallError.ErrorNumber == 3906 ? new SpMaintenanceReadOnlyException(spCall.Method) : spException;
            }
        }

        private async Task<SpCallResult> InvokeCore(SpCall spCall, SpInvokeParamsInternal spi)
        {
            try
            {
                // Check captcha
                await CheckCaptcha(spCall, spi);

                // Call core
                var result = await InvokeSp(spCall, spi);

                // Update result
                await UpdateRecodsetDownloadUri(spCall, spi, result);

                return result;

            }
            catch (Exception ex)
            {
                throw SpExceptionAdapter.Convert(this, ex);
            }
        }

        private async Task<SpCallResult> InvokeSp(SpCall spCall, SpInvokeParamsInternal spi)
        {
            if (!spi.IsSystem && string.IsNullOrWhiteSpace(spi.SpInvokeParams.UserRemoteIp))
            {
                var ex = new ArgumentException(spi.SpInvokeParams.UserRemoteIp, "UserRemoteIp");
                Logger.Log4Net.Error(ex.Message, ex);//Log exception
                throw ex;
            }

            // retrieve user session
            var invokeParams = spi.SpInvokeParams;
            var invokeOptions = spi.SpInvokeParams.InvokeOptions;
            var userSession = SessionManager.GetUserSession(AppName, invokeParams.AuthUserId, invokeParams.Audience);

            //Verify user request limit
            VerifyUserRequestLimit(userSession);

            //call the sp
            var spName = Schema + "." + spCall.Method;
            var spInfo = FindSpInfo(spName);
            if (spInfo == null)
            {
                var ex = new SpException($"Could not find the API: {spName}");
                Logger.Log4Net.Error(ex.Message, ex);//Log exception
                throw ex;
            }

            //check IsCaptcha by meta-data
            if ((spInfo.ExtendedProps.CaptchaMode == SpCaptchaMode.Always || spInfo.ExtendedProps.CaptchaMode == SpCaptchaMode.Auto) && !spi.IsCaptcha)
                throw new SpInvalidCaptchaException(await CaptchaHandler.Create(), spInfo.ProcedureName);

            //check IsBatchAllowed by meta-data
            if (!spInfo.ExtendedProps.IsBatchAllowed && spi.IsBatch)
                throw new SpBatchIsNotAllowedException(spInfo.ProcedureName);

            //Create spCallOptions
            var spCallOptions = new SpCallOptions()
            {
                IsBatch = spi.IsBatch,
                IsCaptcha = spi.IsCaptcha,
                MoneyConversionRate = invokeOptions.MoneyConversionRate,
                RecordIndex = invokeOptions.RecordIndex,
                RecordCount = invokeOptions.RecordCount,
                InvokerAppVersion = AppVersion,
                IsReadonlyIntent = spInfo.ExtendedProps.DataAccessMode == SpDataAccessMode.Read || spInfo.ExtendedProps.DataAccessMode == SpDataAccessMode.ReadSnapshot
            };

            //Get Connection String caring about ReadScale
            var connectionString = GetConnectionString(spInfo, userSession, spi, out bool isWriteMode);

            //create SqlParameters
            var spCallResults = new SpCallResult();
            var sqlParameters = new List<SqlParameter>();

            //set caller params
            using (var sqlConnection = new SqlConnection(connectionString))
            using (var sqlCommand = new SqlCommand(spName, sqlConnection))
            {
                if (spInfo.ExtendedProps.CommandTimeout != -1)
                    sqlCommand.CommandTimeout = spInfo.ExtendedProps.CommandTimeout;

                //set context
                sqlParameters.Add(new SqlParameter("@Context", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = userSession.SpContext.ToString(spCallOptions) });
                sqlParameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });

                //set all sql parameters value
                var spCallParams = spCall.Params ?? new Dictionary<string, object>();
                foreach (var param in spCallParams)
                {
                    //find sqlParam for callerParam
                    var spParam = spInfo.Params.FirstOrDefault(x => x.ParamName.Equals($"@{param.Key}", StringComparison.OrdinalIgnoreCase));
                    if (spParam == null)
                        throw new ArgumentException($"parameter '{param.Key}' does not exists!");

                    spInfo.ExtendedProps.Params.TryGetValue(spParam.ParamName, out SpParamEx spParamEx);

                    //make sure Context has not been set be the caller
                    if (param.Key.Equals("Context", StringComparison.OrdinalIgnoreCase))
                    {
                        var ex = new ArgumentException($"You can not set '{param.Key}' parameter!");
                        Logger.Log4Net.Error(ex.Message, ex);// Log exception
                        throw ex;
                    }
                    // Check jwt token
                    string tokenPayload = CheckJwt(param, spParam, spParamEx);

                    //convert data for db
                    var isMoney = spParamEx != null ? spParamEx.IsUseMoneyConversionRate : false;
                    var value = string.IsNullOrEmpty(tokenPayload) ? param.Value : tokenPayload;
                    object callParamValue = ConvertDataForDb(invokeOptions, spParam.SystemTypeName.ToString(), value, isMoney);

                    //add parameter
                    sqlParameters.Add(new SqlParameter(spParam.ParamName, spParam.SystemTypeName, spParam.Length) { Direction = spParam.IsOutput ? ParameterDirection.InputOutput : ParameterDirection.Input, Value = callParamValue });
                }

                //set all output value which not have been set
                foreach (var spParam in spInfo.Params)
                {
                    if (spParam.IsOutput)
                    {
                        if (string.Equals(spParam.ParamName, "@Recordset", StringComparison.OrdinalIgnoreCase) || string.Equals(spParam.ParamName, "@ReturnValue", StringComparison.OrdinalIgnoreCase))
                            throw new SpException($"{spInfo.ProcedureName} contains {spParam.ParamName} as a output parameter which is not supported!");

                        if (sqlParameters.FirstOrDefault(x => string.Equals(x.ParameterName, spParam.ParamName, StringComparison.OrdinalIgnoreCase)) == null)
                            sqlParameters.Add(new SqlParameter(spParam.ParamName, spParam.SystemTypeName, spParam.Length) { Direction = ParameterDirection.Output });
                    }
                }

                //create command and run it
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
                _dbLayer.OpenConnection(sqlConnection);

                using (var dataReader = await _dbLayer.ExecuteReaderAsync(sqlCommand))
                {

                    //Fill Recordset and close dataReader BEFORE reading sqlParameters
                    ReadRecordset(spCallResults, dataReader, spInfo, invokeOptions);
                    dataReader.Close();

                    //set return value after closing the reader
                    var returnValueParam = sqlParameters.FirstOrDefault(x => string.Equals(x.ParameterName, "@ReturnValue", StringComparison.OrdinalIgnoreCase));
                    if (returnValueParam != null)
                        spCallResults.Add(returnValueParam.ParameterName.Substring(1), returnValueParam.Value != DBNull.Value ? returnValueParam.Value : null);

                    // Build return params
                    foreach (var sqlParam in sqlParameters)
                    {
                        //ignore input parameter
                        if (sqlParam.Direction == ParameterDirection.Input)
                            continue;

                        spInfo.ExtendedProps.Params.TryGetValue(sqlParam.ParameterName, out SpParamEx spParamEx);

                        //process @Context
                        if (sqlParam.ParameterName.Equals("@Context", StringComparison.OrdinalIgnoreCase))
                        {
                            userSession.SpContext = new SpContext((string)sqlParam.Value);
                            continue;
                        }

                        //process @ReturnValue
                        if (sqlParam.ParameterName.Equals("@ReturnValue", StringComparison.OrdinalIgnoreCase))
                            continue; //process after close

                        // Sign text if is need
                        SignJwt(sqlParam, spParamEx);

                        //convert data form db
                        var isMoney = spParamEx != null ? spParamEx.IsUseMoneyConversionRate : false;
                        var value = ConvertDataFromDb(invokeOptions, sqlParam.DbType.ToString(), sqlParam.Value, isMoney);
                        spCallResults.Add(sqlParam.ParameterName.Substring(1), value);

                        // Add Alternative Calendar
                        if (AlternativeIsDateTime(sqlParam.DbType.ToString()))
                            spCallResults.Add(AlternativeGetFieldName(sqlParam.ParameterName.Substring(1)), AlternativeFormatDateTime(value, sqlParam.DbType.ToString()));
                    }
                }

                _dbLayer.CloseConnection(sqlConnection);
            }

            userSession.SetWriteMode(isWriteMode);
            return spCallResults;
        }

        private void SignJwt(SqlParameter sqlParam, SpParamEx spParamEx)
        {
            if (spParamEx?.SignType == SpSignMode.JwtByCertThumb)
                sqlParam.Value = _tokenSigner.Sign(sqlParam.Value.ToString());
        }

        private string CheckJwt(KeyValuePair<string, object> callerParam, SpParam spParam, SpParamEx spParamEx)
        {
            // Sign text if need to sign
            if (spParamEx?.SignType == SpSignMode.JwtByCertThumb && !spParam.IsOutput)
            {
                string token = callerParam.Value.ToString();
                if (string.IsNullOrEmpty(token))
                    return string.Empty;

                if (!_tokenSigner.CheckSign(token))
                    throw new SpInvalidParamSignature(callerParam.Key);

                // Set param value by token payload
                return StringHelper.FromBase64(token.Split('.')[1]);
            }

            return string.Empty;
        }

        private string GetConnectionString(SpInfo spInfo, UserSession userSession, SpInvokeParamsInternal spi, out bool isWriteMode)
        {
            //Select ReadOnly Or Write Connection
            var dataAccessMode = spInfo.ExtendedProps != null ? spInfo.ExtendedProps.DataAccessMode : SpDataAccessMode.Write;

            //Write procedures cannot be called in ForceReadOnly anyway
            if (spi.IsForceReadOnly && dataAccessMode == SpDataAccessMode.Write)
                throw new SpMaintenanceReadOnlyException(spInfo.ProcedureName);

            //Set write request
            isWriteMode = !spi.IsForceReadOnly && dataAccessMode == SpDataAccessMode.Write;

            // Find connection string
            var isSecondary = spi.IsForceReadOnly || dataAccessMode == SpDataAccessMode.ReadSnapshot ||
                (dataAccessMode == SpDataAccessMode.Read && userSession.LastWriteTime.AddSeconds(Options.ReadonlyConnectionSyncInterval) < DateTime.Now);
            return isSecondary ? ConnectionStringReadOnly : ConnectionStringReadWrite;
        }

        public SpInfo FindSpInfo(string spName)
        {
            if (SpInfos.TryGetValue(spName, out SpInfo spInfo))
                return spInfo;

            return null;
        }

        private object ConvertDataForDb(InvokeOptions invokeOptions, string parameterType, object value, bool useMoneyConversionRate)
        {
            if (value == null)
                return DBNull.Value;

            //fix UserString
            if (value is string)
                value = StringHelper.FixUserString((string)value);

            if (parameterType?.ToLower() == "uniqueidentifier")
                return Guid.Parse(value as string);

            if (value is JToken || value is System.Collections.ICollection) //string is an IEnumerable
            {
                if (Options.UseCamelCase)
                    Util.PascalizeJToken(value as JToken);

                value = JsonConvert.SerializeObject(value);
            }

            //convert currency
            if (useMoneyConversionRate)
                value = (float)value / invokeOptions.MoneyConversionRate;

            return value;
        }

        private object ConvertDataFromDb(InvokeOptions invokeOptions, string parameterType, object value, bool useMoneyConversionRate)
        {
            // fix null
            if (value == DBNull.Value)
            {
                return null;
            }

            // try convert json
            if (Util.IsJsonString(value as string))
            {
                try
                {
                    value = JsonConvert.DeserializeObject((string)value);
                    if (Options.UseCamelCase)
                        Util.CamelizeJToken(value as JToken);
                }
                catch { }
            }

            //convert currency
            if (useMoneyConversionRate)
                value = (float)value * invokeOptions.MoneyConversionRate;

            return value;
        }

        private void ReadRecordset(SpCallResult spCallResult, IDataReader dataReader, SpInfo spInfo, InvokeOptions invokeOptions)
        {
            if (dataReader.FieldCount == 0)
                return;

            // Build return recordsetFields
            var fieldInfos = new List<FieldInfo>(dataReader.FieldCount);
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                fieldInfos.Add(new FieldInfo()
                {
                    TypeName = Util.GetFriendlySqlTypeName(((SqlDataReader)dataReader).GetProviderSpecificFieldType(i).Name),
                    IsUseMoneyConversionRate = spInfo.ExtendedProps.Fields.TryGetValue(dataReader.GetName(i), out SpFieldEx spRecodsetFiled) ? spRecodsetFiled.IsUseMoneyConversionRate : false
                });
            }

            // return recordsetFields
            if (invokeOptions.IsWithRecordsetFields)
            {
                var recordsetFields = new Dictionary<string, string>();
                for (int i = 0; i < dataReader.FieldCount; i++)
                    recordsetFields[dataReader.GetName(i)] = fieldInfos[i].TypeName;

                spCallResult.Add("RecordsetFields", recordsetFields);
            }

            // Read to Json object
            if (invokeOptions.RecordsetFormat == RecordsetFormat.Json)
                spCallResult.Recordset = ReadRecordsetAsObject(dataReader, spInfo, fieldInfos.ToArray(), invokeOptions);

            // Read to tabSeparatedValues
            if (invokeOptions.RecordsetFormat == RecordsetFormat.TabSeparatedValues)
                spCallResult.RecordsetText = ReadRecordsetAsTabSeparatedValues(dataReader, spInfo, fieldInfos.ToArray(), invokeOptions);
        }

        private IEnumerable<IDictionary<string, object>> ReadRecordsetAsObject(IDataReader dataReader, SpInfo spInfo, FieldInfo[] fieldInfos, InvokeOptions invokeOptions)
        {
            var recordset = new List<IDictionary<string, object>>();
            while (dataReader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    var itemValue = ConvertDataFromDb(invokeOptions, dataReader.GetDataTypeName(i), dataReader.GetValue(i), fieldInfos[i].IsUseMoneyConversionRate);
                    row.Add(dataReader.GetName(i), itemValue);

                    // Add Alternative Calendar
                    if (AlternativeIsDateTime(fieldInfos[i].TypeName))
                    {
                        row.Add(AlternativeGetFieldName(dataReader.GetName(i)), AlternativeFormatDateTime(itemValue, fieldInfos[i].TypeName));
                    }
                }
                recordset.Add(row);
            }
            return recordset;
        }

        private string ReadRecordsetAsTabSeparatedValues(IDataReader dataReader, SpInfo spInfo, FieldInfo[] fieldInfos, InvokeOptions invokeOptions)
        {
            var stringBuilder = new StringBuilder(1 * 1000000); //1MB

            //add fields
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (i > 0)
                    stringBuilder.Append("\t");

                var fieldName = Options.UseCamelCase ? StringHelper.ToCamelCase(dataReader.GetName(i)) : dataReader.GetName(i);
                stringBuilder.Append(fieldName);

                //AltDateTime
                if (AlternativeIsDateTime(fieldInfos[i].TypeName))
                    stringBuilder.Append($"\t{AlternativeGetFieldName(fieldName)}");
            }
            stringBuilder.AppendLine();

            //add records
            while (dataReader.Read())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append("\t");
                    }

                    var itemValue = ConvertDataFromDb(invokeOptions, dataReader.GetDataTypeName(i), dataReader.GetValue(i), fieldInfos[i].IsUseMoneyConversionRate);
                    string itemValueString = itemValue?.ToString().Trim();

                    // Remove tabs
                    if (itemValue is string)
                    {
                        itemValueString = itemValueString.Replace("\"", "\"\"");
                        itemValueString = itemValueString.Replace("\t", " ");
                        itemValueString = $"\"{itemValueString}\"";

                        // Add ="" if it was a number
                        if (double.TryParse(itemValue.ToString(), out double t))
                            itemValueString = $"={itemValueString}";
                    }

                    if (itemValue is DateTime)
                        itemValueString = ((DateTime)itemValue).ToString("yyyy-MM-dd HH:mm:ss");

                    // Convert json to string
                    if (itemValue is JToken)
                        itemValueString = Util.ToJsonString(itemValue, Options.UseCamelCase);

                    // Write the value
                    stringBuilder.Append(itemValueString);

                    // Write the AltDateTime
                    if (AlternativeIsDateTime(fieldInfos[i].TypeName))
                        stringBuilder.Append("\t" + AlternativeFormatDateTime(itemValue, fieldInfos[i].TypeName));
                }
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        private async Task<bool> CheckCaptcha(SpCall spCall, SpInvokeParamsInternal spi)
        {
            bool ret = false;

            //validate captcha
            if (spi.SpInvokeParams.InvokeOptions.CaptchaId != null || spi.SpInvokeParams.InvokeOptions.CaptchaCode != null)
            {
                await CaptchaHandler.Verify(spi.SpInvokeParams.InvokeOptions.CaptchaId, spi.SpInvokeParams.InvokeOptions.CaptchaCode, spCall.Method);
                spi.IsCaptcha = true;
                ret = true;
            }

            return ret;
        }

        private Task<bool> UpdateRecodsetDownloadUri(SpCall spCall, SpInvokeParamsInternal spi, SpCallResult spCallResult)
        {
            bool result = false;

            var invokeOptions = spi.SpInvokeParams.InvokeOptions;
            if (invokeOptions.IsWithRecodsetDownloadUri)
            {
                //check download
                if (!Options.IsDownloadEnabled)
                    throw new SpAccessDeniedOrObjectNotExistsException();

                var fileTitle = string.IsNullOrWhiteSpace(invokeOptions.RecordsetFileTitle) ?
                    $"{spCall.Method}-{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}" : invokeOptions.RecordsetFileTitle;

                var fileName = $"{fileTitle}.csv";
                var recordSetId = Util.GetRandomString(40);
                string value = null;
                if (invokeOptions.RecordsetFormat == RecordsetFormat.Json)
                {
                    value = Util.ToJsonString(spCallResult.Recordset, Options.UseCamelCase);
                    recordSetId += ".json";
                }
                if (invokeOptions.RecordsetFormat == RecordsetFormat.TabSeparatedValues)
                {
                    value = spCallResult.RecordsetText;
                    recordSetId += ".csv";
                }

                //Cleanup
                CleanTempFolder();

                //Create file in UNC
                var filePath = Path.Combine(InvokerPath.RecordsetsFolder, recordSetId);
                File.WriteAllText(filePath, value, Encoding.Unicode);

                spCallResult.Recordset = null;
                spCallResult.RecordsetText = null;
                spCallResult.RecordsetUri = spi.SpInvokeParams.RecordsetDownloadUrlTemplate.Replace("{id}", recordSetId).Replace("{filename}", fileName);
                result = true;
            }

            return Task.FromResult(result);
        }
        private void CleanTempFolder()
        {
            // Check interval time
            var lifeTime = DateTime.Now.AddSeconds(-Options.DownloadedRecordsetFileLifetime);
            if (LastCleanTempFolderTime != null && LastCleanTempFolderTime > lifeTime)
            {
                return; // Last cleaning was not far
            }

            //clean recordets folder
            var files = Directory.GetFiles(InvokerPath.RecordsetsFolder);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < lifeTime)
                {
                    fi.Delete();
                }
            }

            LastCleanTempFolderTime = DateTime.Now;
        }
        private string AlternativeFormatDateTime(object fieldValue, string typeName)
        {
            return fieldValue == null ? null : ((DateTime)fieldValue).ToString(typeName.ToLower() == "date" ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss", Options.AlternativeCalendar);
        }
        private string AlternativeGetFieldName(string fieldName)
        {
            return $"{fieldName}_{Options.AlternativeCalendar.TwoLetterISOLanguageName}";
        }
        private bool AlternativeIsDateTime(string typeName)
        {
            return typeName.ToLower().Substring(0, 4) == "date" && Options.AlternativeCalendar != null;
        }

        private async Task CheckDuplicateRequest(string requestId, int commandTimeout = 30)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                return;
            }

            // 0 is treated as 2 hours
            if (commandTimeout == 0)
            {
                commandTimeout = 2 * 3600;
            }

            // Calculating time to life base on sp command time out
            int timeToLife = Math.Max(commandTimeout * 2, 15 * 60); //the minimum value of timeToLife is 15 min

            try
            {
                await KeyValue.SetValue($"RequestId/{requestId}", "", timeToLife, false);
            }
            catch (SpObjectAlreadyExists)
            {
                throw new SpDuplicateRequestException(requestId);
            }
        }
    }
}
