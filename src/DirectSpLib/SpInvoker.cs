using DirectSpLib.Entities;
using DirectSpLib.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http;
using DirectSpLib.InternalDb;

namespace DirectSpLib
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

        private SpContext _AppUserContext;
        public SpContext AppUserContext
        {
            get
            {
                if (_AppUserContext == null)
                {
                    using (var sqlConnection = new SqlConnection(ConnectionStringReadOnly))
                    {
                        sqlConnection.Open();
                        _AppUserContext = new SpContext(ResourceDb.System_AppUserContext(sqlConnection));
                    }
                }
                return _AppUserContext;
            }
        }

        public string AppName { get { return AppUserContext.AppName; } }

        // User Request Count control
        private void VerifyUserRequestLimit(UserSession userSession)
        {
            //AppUserId does not have request limit
            if (userSession.UserId == AppUserContext.UserId)
                return;

            //Reset ResetRequestCount
            if (userSession.RequestIntervalStartTime.AddSeconds(Options.SessionMaxRequestCycleInterval) < DateTime.Now)
                userSession.ResetRequestCount();

            //Reject Request
            if (userSession.RequestCount > Options.SessionMaxRequestCount)
                throw new SpException("Too many request! Please try a few minutes later!", StatusCodes.Status429TooManyRequests);
        }

        //Updateing StoreProcedures
        public DateTime StoreProceduresUpdateTime { get; private set; }
        private Dictionary<string, SpInfo> _StoreProcedures;
        public Dictionary<string, SpInfo> SqlStoreProcedures
        {
            get
            {
                if (_StoreProcedures == null || StoreProceduresUpdateTime.AddSeconds(Options.SqlStoreProceduresUpdateInterval) < DateTime.Now)
                {
                    StoreProceduresUpdateTime = DateTime.Now;
                    _StoreProcedures = ReadStoreProcedures();
                }
                return _StoreProcedures;
            }
        }
        private Dictionary<string, SpInfo> ReadStoreProcedures()
        {
            var storeProcedures = new Dictionary<string, SpInfo>();
            using (var sqlConnection = new SqlConnection(ConnectionStringReadOnly))
            {
                sqlConnection.Open();
                var spList = ResourceDb.System_Api(sqlConnection, AppUserContext.ToString());
                foreach (var item in spList)
                    storeProcedures.Add(item.SchemaName + "." + item.ProcedureName, item);
                _StoreProcedures = storeProcedures;
            }

            return _StoreProcedures;
        }

        public async Task<IEnumerable<SpCallResult>> Invoke(SpCall[] spCalls, SpInvokeParams spInvokeParams)
        {
            var spi = new SpInvokeParamsInternal
            {
                SpInvokeParams = spInvokeParams,
                IsBatch = true
            };

            var spCallResults = new List<SpCallResult>();
            foreach (var spCall in spCalls)
            {
                try
                {
                    //batch
                    var spCallResult = await Invoke(spCall, spi);
                    spCallResults.Add(spCallResult);
                }
                catch (SpException ex)
                {
                    if (ex.StatusCode == StatusCodes.Status500InternalServerError)
                        throw ex;

                    //add error object
                    var spCallResult = new SpCallResult { { "error", ex.SpCallError } };
                    spCallResults.Add(spCallResult);
                }
            }

            return spCallResults;
        }

        public async Task<SpCallResult> Invoke(SpCall spCall)
        {
            var spInvokeParams = new SpInvokeParams();
            return await Invoke(spCall, spInvokeParams, true);
        }

        public async Task<SpCallResult> Invoke(SpCall spCall, SpInvokeParams spInvokeParams, bool isSystem = false)
        {
            var spi = new SpInvokeParamsInternal { SpInvokeParams = spInvokeParams, IsSystem = isSystem };
            if (isSystem)
                spi.SpInvokeParams.UserId = AppUserContext.UserId;
            return await Invoke(spCall, spi);
        }

        private async Task<SpCallResult> Invoke(SpCall spCall, SpInvokeParamsInternal spi)
        {
            try
            {
                //validate captcha
                await ValidateCaptcha(spi);

                // call core
                var result = await InvokeImpl(spCall, spi);

                // Update result
                await UpdateRecodsetDownloadUri(spCall, spi, result);

                return result;

            }
            catch (Exception ex)
            {
                throw SpExceptionBuilder.Create(InternalSpInvoker, ex);
            }
        }

        private async Task<SpCallResult> InvokeImpl(SpCall spCall, SpInvokeParamsInternal spi)
        {
            if (!spi.IsSystem && string.IsNullOrWhiteSpace(spi.SpInvokeParams.UserRemoteIp))
                throw new ArgumentException(spi.SpInvokeParams.UserRemoteIp, "UserRemoteIp");

            // retrieve user session
            var invokeParams = spi.SpInvokeParams;
            var invokeOptions = spi.SpInvokeParams.InvokeOptions;
            var userSession = SessionManager.GetUserSession(AppName, invokeParams.UserId, invokeParams.Audience);

            //Verify user request limit
            VerifyUserRequestLimit(userSession);

            //call the sp
            var spName = Schema + "." + spCall.Method;
            var spInfo = FindSpInfo(spName);
            if (spInfo == null)
                throw new Exception($"Could not find the SP: {spName}");

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
                RecordCount = invokeOptions.RecordCount
            };

            //Select Connection
            var connectionString = ConnectionStringReadWrite;
            var executeMode = spInfo.ExtendedProps != null ? spInfo.ExtendedProps.ExecuteMode : SpExecuteMode.Write;
            if (executeMode == SpExecuteMode.ReadSnapshot) connectionString = ConnectionStringReadOnly;
            else if (executeMode == SpExecuteMode.ReadWise && userSession.LastWriteTime.AddSeconds(Options.ReadonlyConnectionSyncInterval) < DateTime.Now)
                connectionString = ConnectionStringReadOnly;
            userSession.SetCurrentRequestMode(executeMode == SpExecuteMode.Write);

            //create SqlParameters
            var spCallResults = new SpCallResult();
            var sqlParameters = new List<SqlParameter>();

            //set caller params
            using (var sqlConn = new SqlConnection(connectionString))
            using (var command = new SqlCommand(spName, sqlConn))
            {
                if (spInfo.ExtendedProps.CommandTimeout != -1)
                    command.CommandTimeout = spInfo.ExtendedProps.CommandTimeout;

                //set context
                sqlParameters.Add(new SqlParameter("@Context", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = userSession.SpContext.ToString(spCallOptions) });
                sqlParameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });

                //set all sql parameters value
                var spCallParams = spCall.Params ?? new Dictionary<string, object>();
                foreach (var callerParam in spCallParams)
                {
                    //find sqlParam for callerParam
                    var spParam = spInfo.Params.FirstOrDefault(x => x.ParamName.Equals("@" + callerParam.Key, StringComparison.OrdinalIgnoreCase));
                    if (spParam == null)
                        throw new ArgumentException($"parameter '{callerParam.Key}' does not exists!");

                    //make sure Context has not been set be the caller
                    if (callerParam.Key.Equals("Context", StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException($"You can not set '{callerParam.Key}' parameter!");

                    //convert data for db
                    var isMoney = spInfo.ExtendedProps.Params.TryGetValue(spParam.ParamName, out SpParamEx spParamEx) ? spParamEx.IsUseMoneyConversionRate : false;
                    object callParamValue = ConvertDataForDb(invokeOptions, spParam.SystemTypeName.ToString(), callerParam.Value, isMoney);

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
                sqlConn.Open();
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(sqlParameters.ToArray());

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    // Build return params
                    spCallResults = new SpCallResult();
                    foreach (var sqlParam in sqlParameters)
                    {
                        //ignore input parameter
                        if (sqlParam.Direction == ParameterDirection.Input)
                            continue;

                        //process @Context
                        if (sqlParam.ParameterName.Equals("@Context", StringComparison.OrdinalIgnoreCase))
                        {
                            userSession.SpContext = new SpContext((string)sqlParam.Value);
                            continue;
                        }

                        //process @ReturnValue
                        if (sqlParam.ParameterName.Equals("@ReturnValue", StringComparison.OrdinalIgnoreCase))
                            continue; //process after close

                        //convert data form db
                        var isMoney = spInfo.ExtendedProps.Params.TryGetValue(sqlParam.ParameterName, out SpParamEx spParamEx) ? spParamEx.IsUseMoneyConversionRate : false;
                        var value = ConvertDataFromDb(invokeOptions, sqlParam.DbType.ToString(), sqlParam.Value, isMoney);
                        spCallResults.Add(sqlParam.ParameterName.Substring(1), value);

                        // Add Alternative Calendar
                        if (AltDateTime_IsDateTime(sqlParam.DbType.ToString()))
                            spCallResults.Add(AltDateTime_GetFieldName(sqlParam.ParameterName.Substring(1)), AltDateTime_GetFieldValue(value, sqlParam.DbType.ToString()));
                    }

                    //Fill Recordset
                    ReadRecordset(spCallResults, dataReader, spInfo, invokeOptions);
                }

                //set return value after closing the reader
                var returnValueParam = sqlParameters.FirstOrDefault(x => string.Equals(x.ParameterName, "@ReturnValue", StringComparison.OrdinalIgnoreCase));
                if (returnValueParam != null)
                    spCallResults.Add(returnValueParam.ParameterName.Substring(1), returnValueParam.Value != DBNull.Value ? returnValueParam.Value : null);

                sqlConn.Close();
            }
            return spCallResults;
        }

        public SpInfo FindSpInfo(string spName)
        {
            if (SqlStoreProcedures.TryGetValue(spName, out SpInfo sqlSp))
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

                var converter = invokeOptions.IsAntiXss ? new AntiXssConverter(true) : null;
                value = JsonConvert.SerializeObject(value, converter);
            }

            //check xss
            else if (value is string)
            {
                if (invokeOptions.IsAntiXss)
                    Util.AntiXss_VerifyValue(value as string, true);
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

        private void ReadRecordset(SpCallResult spCallResult, SqlDataReader dataReader, SpInfo spInfo, InvokeOptions invokeOptions)
        {
            if (dataReader.FieldCount == 0)
                return;

            // Build return recordsetFields
            var fieldInfos = new List<FieldInfo>(dataReader.FieldCount);
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                fieldInfos.Add(new FieldInfo()
                {
                    TypeName = Util.GetFriendlySqlTypeName(dataReader.GetProviderSpecificFieldType(i).Name),
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

        private IEnumerable<IDictionary<string, object>> ReadRecordsetAsObject(SqlDataReader dataReader, SpInfo spInfo, FieldInfo[] fieldInfos, InvokeOptions invokeOptions)
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
                    if (AltDateTime_IsDateTime(fieldInfos[i].TypeName))
                        row.Add(AltDateTime_GetFieldName(dataReader.GetName(i)), AltDateTime_GetFieldValue(itemValue, fieldInfos[i].TypeName));
                }
                recordset.Add(row);
            }
            return recordset;
        }

        private string ReadRecordsetAsTabSeparatedValues(SqlDataReader dataReader, SpInfo spInfo, FieldInfo[] fieldInfos, InvokeOptions invokeOptions)
        {
            var strBuilder = new StringBuilder(1 * 1000000); //1MB

            //add fields
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (i > 0)
                    strBuilder.Append("\t");
                var fieldName = Options.UseCamelCase ? Util.ToCamelCase(dataReader.GetName(i)) : dataReader.GetName(i);
                strBuilder.Append(fieldName);

                //AltDateTime
                if (AltDateTime_IsDateTime(fieldInfos[i].TypeName))
                    strBuilder.Append("\t" + AltDateTime_GetFieldName(fieldName));
            }
            strBuilder.AppendLine();

            //add records
            while (dataReader.Read())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (i > 0)
                        strBuilder.Append("\t");
                    var itemValue = ConvertDataFromDb(invokeOptions, dataReader.GetDataTypeName(i), dataReader.GetValue(i), fieldInfos[i].IsUseMoneyConversionRate);
                    string itemValueString = itemValue?.ToString();

                    //remove tabs
                    if (itemValue is string)
                    {
                        itemValueString = $"=\"{itemValue}\"";
                        itemValueString = itemValueString.ToString().Replace("\t", " ");
                    }

                    if (itemValue is DateTime)
                        itemValueString = ((DateTime)itemValue).ToString("yyyy-MM-dd HH:mm:ss");

                    // convert json to string
                    if (itemValue is JToken)
                        itemValueString = Util.ToJsonString(itemValue, Options.UseCamelCase);

                    //write the value
                    strBuilder.Append(itemValueString);

                    //write the AltDateTime
                    if (AltDateTime_IsDateTime(fieldInfos[i].TypeName))
                        strBuilder.Append("\t" + AltDateTime_GetFieldValue(itemValue, fieldInfos[i].TypeName));
                }
                strBuilder.AppendLine();
            }

            return strBuilder.ToString();
        }

        private async Task<bool> ValidateCaptcha(SpInvokeParamsInternal spi)
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
            bool ret = false;

            var invokeOptions = spi.SpInvokeParams.InvokeOptions;
            if (invokeOptions.IsWithRecodsetDownloadUri)
            {
                var fileTitle = string.IsNullOrWhiteSpace(invokeOptions.RecordsetFileTitle) ? spCall.Method + "-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") : invokeOptions.RecordsetFileTitle;
                var fileName = fileTitle + ".csv";
                var recordsetId = Util.GetRandomString(40);
                var value = invokeOptions.RecordsetFormat == RecordsetFormat.Json ? Util.ToJsonString(spCallResult.Recordset, Options.UseCamelCase) : spCallResult.RecordsetText;
                await KeyValue.ValueSet("recordset/" + recordsetId, value, Options.DownloadedRecordsetFileLifetime);
                spCallResult.Recordset = null;
                spCallResult.RecordsetText = null;
                spCallResult.RecordsetUri = spi.SpInvokeParams.RecordsetDownloadUrlTemplate.Replace("{id}", recordsetId).Replace("{filename}", fileName);
                spi.SpInvokeParams.InvokeOptions.IsAntiXss = false; //prevent encoding url
                ret = true;
            }

            return ret;
        }

        private string AltDateTime_GetFieldValue(object fieldValue, string typeName)
        {
            return fieldValue == null ? null : ((DateTime)fieldValue).ToString(typeName.ToLower() == "date" ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss", Options.AlternativeCalendar);
        }
        private string AltDateTime_GetFieldName(string fieldName)
        {
            return fieldName + "_" + Options.AlternativeCalendar.TwoLetterISOLanguageName;
        }

        private bool AltDateTime_IsDateTime(string typeName)
        {
            return typeName.ToLower().Substring(0, 4) == "date" && Options.AlternativeCalendar != null;
        }
    }
}
