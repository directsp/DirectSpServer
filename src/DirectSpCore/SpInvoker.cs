using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http;
using DirectSp.Core.InternalDb;
using System.IO;
using DirectSp.Core.SpSchema;
using DirectSp.Core.DI;
using DirectSp.Core.Infrastructure;
using System.Collections.Concurrent;
using System.Threading;
using DirectSp.Core.Helpers;

namespace DirectSp.Core
{
    public class SpInvoker
    {
        public SpInvokerOptions Options { get; private set; }
        public string ConnectionString { get; private set; }
        public string Schema { get; private set; }
        public string ConnectionStringReadOnly { get { return new SqlConnectionStringBuilder(ConnectionString) { ApplicationIntent = ApplicationIntent.ReadOnly }.ToString(); } }
        public string ConnectionStringReadWrite { get { return new SqlConnectionStringBuilder(ConnectionString) { ApplicationIntent = ApplicationIntent.ReadWrite }.ToString(); } }
        public DspKeyValue KeyValue { get; private set; }
        private UserSessionManager SessionManager;
        private SpInvoker InternalSpInvoker;

        public SpInvoker(string connectionString, string schema, SpInvokerOptions options, SpInvoker spInvokerInternal = null)
        {
            //validate ConnectionString
            Schema = schema ?? throw new Exception("Schema is not set!");
            ConnectionString = connectionString ?? throw new Exception("ConnectionString is not set!");
            var connStringBuilder = new SqlConnectionStringBuilder(connectionString);

            if (connectionString.IndexOf("ApplicationIntent", StringComparison.OrdinalIgnoreCase) != -1)
                throw new Exception("ConnectionString should have a ApplicationIntent parameter!");

            Options = options;
            InternalSpInvoker = spInvokerInternal ?? this;
            SessionManager = new UserSessionManager(options);
            KeyValue = new DspKeyValue(spInvokerInternal);
            SpException.UseCamelCase = options.UseCamelCase;
        }

        public string RecordsetsFolerPath
        {
            get { return string.IsNullOrWhiteSpace(Options.TempFolderPath) ? null : Path.Combine(Options.TempFolderPath, "recordsets"); }
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
                        RefreshApi();
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
                        RefreshApi();
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
                return;

            //Reset ResetRequestCount
            if (userSession.RequestIntervalStartTime.AddSeconds(Options.SessionMaxRequestCycleInterval) < DateTime.Now)
                userSession.ResetRequestCount();

            //Reject Request
            if (userSession.RequestCount > Options.SessionMaxRequestCount)
                throw new SpException("Too many request! Please try a few minutes later!", StatusCodes.Status429TooManyRequests);
        }

        private object LockObject = new object();

        private void RefreshApi()
        {
            lock (LockObject)
            {
                var spInfos = new Dictionary<string, SpInfo>();
                using (var sqlConnection = new SqlConnection(ConnectionStringReadOnly))
                {
                    var spList = ResourceDb.System_Api(sqlConnection, out string appUserContext);
                    foreach (var item in spList)
                        spInfos.Add(item.SchemaName + "." + item.ProcedureName, item);

                    _SpInfos = spInfos;
                    _AppUserContext = new SpContext(appUserContext, "$$");
                    AppVersion = _AppUserContext.AppVersion; //don't make AppVersion property because _AppUserContext may not be initialized when there is error
                }
            }
        }

        public Task<SpCallResult[]> Invoke(SpCall[] spCalls, SpInvokeParams spInvokeParams)
        {
            var spi = new SpInvokeParamsInternal
            {
                SpInvokeParams = spInvokeParams,
                IsBatch = true
            };

            var spCallResults = new List<SpCallResult>();
            var tasks = new List<Task<SpCallResult>>();
            foreach (var spCall in spCalls)
                tasks.Add(Invoke(spCall, spi));

            try
            {
                tasks.ForEach(task => task.Start());
                Task.WaitAll(tasks.ToArray());
            }
            catch
            {

            }
            finally
            {
                foreach (var item in tasks)
                {
                    if (item.IsCompletedSuccessfully)
                        spCallResults.Add(item.Result);
                    else if (item.IsFaulted)
                        spCallResults.Add(new SpCallResult { { "error", item.Exception.Message } });
                }
                Console.WriteLine("OK");
            }
            return Task.FromResult(spCallResults.ToArray());
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
            // create spCall
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
            catch (SpMaintenanceReadOnlyException ex)
            {
                try
                {
                    spi.IsForceReadOnly = true;
                    return await InvokeCore(spCall, spi);
                }
                catch (SpException spException)
                {
                    throw spException.SpCallError.ErrorNumber == 3906 ? ex : spException;
                }
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
                await CheckCaptcha(spi);

                // Call core
                var result = await InvokeSp(spCall, spi);

                // Update result
                await UpdateRecodsetDownloadUri(spCall, spi, result);

                return result;

            }
            catch (Exception ex)
            {
                throw SpExceptionBuilder.Create(InternalSpInvoker, ex);
            }
        }

        private async Task<SpCallResult> InvokeSp(SpCall spCall, SpInvokeParamsInternal spi)
        {
            if (!spi.IsSystem && string.IsNullOrWhiteSpace(spi.SpInvokeParams.UserRemoteIp))
                throw new ArgumentException(spi.SpInvokeParams.UserRemoteIp, "UserRemoteIp");

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
                throw new SpException($"Could not find the API: {spName}");

            //check IsCaptcha by meta-data
            if ((spInfo.ExtendedProps.CaptchaMode == SpCaptchaMode.Always || spInfo.ExtendedProps.CaptchaMode == SpCaptchaMode.Auto) && !spi.IsCaptcha)
                throw new SpInvalidCaptchaException(InternalSpInvoker, spInfo.ProcedureName);

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
            };

            //Get Connection String caring about ReadScale
            var connectionString = GetConnectionString(spInfo, userSession, spi);

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
                        throw new ArgumentException($"You can not set '{param.Key}' parameter!");

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
                        if (string.Equals(spParam.ParamName, "@Recordset", StringComparison.OrdinalIgnoreCase) || string.Equals(spParam.ParamName, "@ReturnValue", StringComparison.OrdinalIgnoreCase)) throw new SpException($"{spInfo.ProcedureName} contains {spParam.ParamName} as a output parameter which is not supported!");
                        if (sqlParameters.FirstOrDefault(x => string.Equals(x.ParameterName, spParam.ParamName, StringComparison.OrdinalIgnoreCase)) == null)
                            sqlParameters.Add(new SqlParameter(spParam.ParamName, spParam.SystemTypeName, spParam.Length) { Direction = ParameterDirection.Output });
                    }
                }

                //create command and run it
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
                var dbLayer = Resolver.Instance.Resolve<IDbLayer>();
                dbLayer.OpenConnection(sqlConnection);

                using (var dataReader = await dbLayer.ExecuteReaderAsync(sqlCommand))
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

                dbLayer.CloseConnection(sqlConnection);
            }
            return spCallResults;
        }

        private static void SignJwt(SqlParameter sqlParam, SpParamEx spParamEx)
        {
            if (spParamEx?.SignType == SpSignMode.JwtByCertThumb)
            {
                var tokenSigner = Resolver.Instance.Resolve<JwtTokenSigner>();
                sqlParam.Value = tokenSigner.Sign(sqlParam.Value.ToString());
            }
        }

        private static string CheckJwt(KeyValuePair<string, object> callerParam, SpParam spParam, SpParamEx spParamEx)
        {
            // Sign text if need to sign
            if (spParamEx?.SignType == SpSignMode.JwtByCertThumb && !spParam.IsOutput)
            {
                string token = callerParam.Value.ToString();
                if (string.IsNullOrEmpty(token)) return string.Empty;
                var tokenSigner = Resolver.Instance.Resolve<JwtTokenSigner>();
                if (!tokenSigner.CheckSign(token))
                    throw new SpInvalidParamSignature(callerParam.Key);
                // Set param value by token payload
                return token.Split('.')[1].FromBase64();
            }

            return string.Empty;
        }

        private string GetConnectionString(SpInfo spInfo, UserSession userSession, SpInvokeParamsInternal spi)
        {
            //Select ReadOnly Or Write Connection
            var executeMode = spInfo.ExtendedProps != null ? spInfo.ExtendedProps.ExecuteMode : SpExecuteMode.NotSet;

            //Write procedures cannot be called in ForceReadOnly anyway
            if (spi.IsForceReadOnly && executeMode == SpExecuteMode.Write)
                throw new SpMaintenanceReadOnlyException(spInfo.ProcedureName);

            //Set write request
            userSession.SetWriteMode(!spi.IsForceReadOnly && (executeMode == SpExecuteMode.NotSet || executeMode == SpExecuteMode.Write));

            // Find connection string
            var isSecondary = spi.IsForceReadOnly || executeMode == SpExecuteMode.ReadSnapshot ||
                (executeMode == SpExecuteMode.ReadWise && userSession.LastWriteTime.AddSeconds(Options.ReadonlyConnectionSyncInterval) < DateTime.Now);
            return isSecondary ? ConnectionStringReadOnly : ConnectionStringReadWrite;
        }

        public SpInfo FindSpInfo(string spName)
        {
            if (SpInfos.TryGetValue(spName, out SpInfo sqlSp))
                return sqlSp;
            return null;
        }

        private object ConvertDataForDb(InvokeOptions invokeOptions, string parameterType, object value, bool useMoneyConversionRate)
        {
            if (value == null)
                return DBNull.Value;

            //fix UserString
            if (value is string)
                value = Util.FixUserString((string)value);

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
                return null;

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
                spCallResult.RecordsetText = ReadRecordsetByTab(dataReader, spInfo, fieldInfos.ToArray(), invokeOptions);
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
                        row.Add(AlternativeGetFieldName(dataReader.GetName(i)), AlternativeFormatDateTime(itemValue, fieldInfos[i].TypeName));
                }
                recordset.Add(row);
            }
            return recordset;
        }

        private string ReadRecordsetByTab(IDataReader dataReader, SpInfo spInfo, FieldInfo[] fieldInfos, InvokeOptions invokeOptions)
        {
            var stringBuilder = new StringBuilder(1 * 1000000); //1MB

            //add fields
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (i > 0)
                    stringBuilder.Append("\t");
                var fieldName = Options.UseCamelCase ? Util.ToCamelCase(dataReader.GetName(i)) : dataReader.GetName(i);
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
                        stringBuilder.Append("\t");
                    var itemValue = ConvertDataFromDb(invokeOptions, dataReader.GetDataTypeName(i), dataReader.GetValue(i), fieldInfos[i].IsUseMoneyConversionRate);
                    string itemValueString = itemValue?.ToString().Trim();

                    // Remove tabs
                    if (itemValue is string)
                    {
                        itemValueString = itemValueString.Replace("'", "''");
                        itemValueString = itemValueString.Replace("\t", " ");
                        itemValueString = $"'{itemValueString}'";
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

        #region helpers
        private async Task<bool> CheckCaptcha(SpInvokeParamsInternal spi)
        {
            bool ret = false;

            //validate captcha
            if (spi.SpInvokeParams.InvokeOptions.CaptchaId != null || spi.SpInvokeParams.InvokeOptions.CaptchaCode != null)
            {
                var captcha = new Captcha(InternalSpInvoker);
                await captcha.Match(spi.SpInvokeParams.InvokeOptions.CaptchaId, spi.SpInvokeParams.InvokeOptions.CaptchaCode);
                spi.IsCaptcha = true;
                ret = true;
            }

            return ret;
        }
        private async Task<bool> UpdateRecodsetDownloadUri(SpCall spCall, SpInvokeParamsInternal spi, SpCallResult spCallResult)
        {
            bool result = false;

            var invokeOptions = spi.SpInvokeParams.InvokeOptions;
            if (invokeOptions.IsWithRecodsetDownloadUri)
            {
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

                //create file
                if (RecordsetsFolerPath != null)
                {
                    //Cleanup
                    CleanTempFolder();

                    //create file in UNC
                    var filePath = Path.Combine(RecordsetsFolerPath, recordSetId);
                    File.WriteAllText(filePath, value, Encoding.Unicode);
                }
                else
                {
                    //create file in DB
                    await KeyValue.ValueSet($"recordset/{recordSetId}", value, Options.DownloadedRecordsetFileLifetime);
                }

                spCallResult.Recordset = null;
                spCallResult.RecordsetText = null;
                spCallResult.RecordsetUri = spi.SpInvokeParams.RecordsetDownloadUrlTemplate.Replace("{id}", recordSetId).Replace("{filename}", fileName);
                spi.SpInvokeParams.InvokeOptions.IsAntiXss = false; //prevent encoding url
                result = true;
            }

            return result;
        }
        private void CleanTempFolder()
        {
            if (RecordsetsFolerPath == null)
                return;

            // Check interval time
            var lifeTime = DateTime.Now.AddSeconds(-Options.DownloadedRecordsetFileLifetime);
            if (LastCleanTempFolderTime != null && LastCleanTempFolderTime > lifeTime)
                return; // Last cleaning was not far

            // InitFolder
            Directory.CreateDirectory(Options.TempFolderPath);
            Directory.CreateDirectory(RecordsetsFolerPath);

            //clean temp folder
            var files = Directory.GetFiles(RecordsetsFolerPath);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < lifeTime)
                    fi.Delete();
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
        #endregion helper
    }
}
