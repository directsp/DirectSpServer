using DirectSp.ProcedureInfos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DirectSp.Providers
{
    public class ObjectCommandProvider : ICommandProvider
    {
        private readonly Type _targetType;
        private readonly object _targetObject;
        private readonly SpInfo[] _spInfos;
        public ObjectCommandProvider(object obj)
            : this(obj, obj.GetType())
        {
        }

        public ObjectCommandProvider(object targetObject, Type targetType)
        {
            _targetType = targetType;
            _targetObject = targetObject;
            _spInfos = SpInfos_FromType(targetType);
        }

        public async Task<CommandResult> Execute(SpInfo procInfo, IDictionary<string, object> callParams, bool isReadScale)
        {
            var schema = procInfo.SchemaName;
            var procName = procInfo.ProcedureName;

            if (schema != _targetType.Name)
                throw new Exception($"Invalid schema {schema}!");

            var methodInfo = _targetType.GetMethod(procName);
            if (methodInfo == null) throw new Exception($"{procName} was not found!");

            //call method by name parameters
            var parameterInfos = methodInfo.GetParameters();
            var parameterValues = new object[parameterInfos.Length];
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                if (callParams.TryGetValue(parameterInfo.Name, out object value) && value != Undefined.Value)
                {
                    parameterValues[i] = value;
                }
                else if (parameterInfo.HasDefaultValue)
                {
                    parameterValues[i] = parameterInfo.DefaultValue;
                }
            }

            //throw error if there is additional params in given parameters
            foreach (var callParam in callParams)
            {
                if (callParam.Key != "returnValue" && parameterInfos.FirstOrDefault(x => x.Name == callParam.Key) == null)
                    throw new Exception($"Unknown parameter! ParameterName: {callParam.Key}");
            }

            var ret = new CommandResult();

            try
            {
                //invoke command and set return value
                if (methodInfo.ReturnType == typeof(void))
                    methodInfo.Invoke(_targetObject, parameterValues);
                else
                {
                    var invokeRes = methodInfo.Invoke(_targetObject, parameterValues);

                    //manage result for Task (void)
                    if (invokeRes is Task && !invokeRes.GetType().IsGenericType)
                    {
                        await (Task)invokeRes;
                    }
                    //manage result for Task (generic)
                    else if (invokeRes is Task)
                    {
                        await ((Task)invokeRes).ConfigureAwait(false);
                        ret.OutParams["returnValue"] = ((Task)invokeRes).GetType().GetProperty("Result").GetValue(invokeRes);

                    }
                    else
                        ret.OutParams["returnValue"] = invokeRes;
                }

                //set output paramters
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    if (parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef)
                    {
                        if (callParams.ContainsKey(parameterInfo.Name))
                            ret.OutParams[parameterInfo.Name] = parameterValues[i];
                    }
                }

                return ret;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<SpInfo[]> GetSystemApi(out string context)
        {
            var ctx = new InvokeContext(appName: _targetType.Name, authUserId: "$$", audience: null);
            context = JsonConvert.SerializeObject(ctx);
            return Task.FromResult(_spInfos);
        }

        private SpInfo[] SpInfos_FromType(Type type)
        {
            var spInfos = new List<SpInfo>();
            foreach (var methodInfo in type.GetMethods())
            {
                var spInfo = ProcInfo_FromMethodInfo(methodInfo);
                spInfo.SchemaName = type.Name;
                spInfos.Add(spInfo);
            }

            return spInfos.ToArray();
        }

        private SpParamInfo ProcInfo_GetParamInfo(ParameterInfo paramInfo, Dictionary<string, SpParamInfoEx> paramsEx)
        {
            var isRetParam = paramInfo.Position == -1;

            var spParamInfo = new SpParamInfo
            {
                ParamName = isRetParam ? "returnValue" : paramInfo.Name,
                IsOutput = paramInfo.IsOut || paramInfo.IsRetval || isRetParam,
                IsOptional = paramInfo.IsOptional,
                DefaultValue = paramInfo.DefaultValue,
                UserTypeName = paramInfo.ParameterType.Name,
                SystemTypeName = paramInfo.ParameterType.Name
            };

            //extended paramInfo
            var paramAttribute = paramInfo.GetCustomAttribute<DirectSpParamAttribute>() ?? new DirectSpParamAttribute();
            var spParamInfoEx = new SpParamInfoEx()
            {
                SignType = paramAttribute.SignType
            };

            paramsEx[spParamInfo.ParamName] = spParamInfoEx;

            return spParamInfo;
        }

        private SpInfo ProcInfo_FromMethodInfo(MethodInfo methodInfo)
        {
            var paramsEx = new Dictionary<string, SpParamInfoEx>();

            //add paramInfo
            var spParamInfos = new List<SpParamInfo>();
            foreach (var paramInfo in methodInfo.GetParameters())
                spParamInfos.Add(ProcInfo_GetParamInfo(paramInfo, paramsEx));

            //add return value as an out parameter
            if (methodInfo.ReturnParameter.ParameterType != typeof(void))
                spParamInfos.Add(ProcInfo_GetParamInfo(methodInfo.ReturnParameter, paramsEx));

            // add method attributes
            var directSpAttribute = methodInfo.GetCustomAttribute<DirectSpProcAttribute>() ?? new DirectSpProcAttribute();

            //create procedure infos
            var spInfo = new SpInfo
            {
                ProcedureName = methodInfo.Name,
                Params = spParamInfos.ToArray(),
                ExtendedProps = new SpInfoEx()
                {
                    IsBatchAllowed = directSpAttribute.IsBatchAllowed,
                    CaptchaMode = directSpAttribute.CaptchaMode,
                    Params = paramsEx
                }
            };

            return spInfo;
        }
    }
}
