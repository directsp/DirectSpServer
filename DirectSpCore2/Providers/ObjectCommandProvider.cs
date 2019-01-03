using DirectSp.Core.ProcedureInfos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DirectSp.Core.Providers
{
    public class ObjectCommandProvider : ICommandProvider
    {
        private readonly Type _type;
        private readonly object _object;
        private readonly SpInfo[] _spInfos;
        public ObjectCommandProvider(object obj)
            : this(obj, obj.GetType())
        {
        }

        public ObjectCommandProvider(object obj, Type type)
        {
            _type = type;
            _spInfos = SpInfos_FromType(type);
            _object = obj;
        }

        public Task<CommandResult> Execute(SpInfo procInfo, IDictionary<string, object> callParams, bool isReadScale)
        {
            var schema = procInfo.SchemaName;
            var procName = procInfo.ProcedureName;

            if (schema != _type.Name)
                throw new Exception($"Invalid schema {schema}!");

            var typeInfo = _type;

            if (procName == "getProperty")
            {
                var propName = (string)callParams["name"];
                var propertyInfo = typeInfo.GetProperty(procName);
                if (propertyInfo == null) throw new ArgumentException($"{propName} property does not found!");
                var result = new CommandResult();
                result.OutParams["ReturnValue"] = propertyInfo.GetValue(_object);
                return Task.FromResult(result);
            }
            else if (procName == "setProperty")
            {
                var propName = (string)callParams["name"];
                var propValue = (string)callParams["value"];
                var propertyInfo = typeInfo.GetProperty(propName);
                if (propertyInfo == null) throw new Exception($"{propName} property not found!");
                propertyInfo.SetValue(this, Convert.ChangeType(propValue, propertyInfo.PropertyType));
                return Task.FromResult((CommandResult)null);
            }
            else
            {
                var methodInfo = typeInfo.GetMethod(procName);
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
                    if (parameterInfos.FirstOrDefault(x => x.Name == callParam.Key) == null)
                        throw new Exception($"Unknown parameter! ParameterName: {callParam.Key}");
                }

                var ret = new CommandResult();
                var result = methodInfo.Invoke(_object, parameterValues);

                //set return value
                ret.ReturnValue = result;

                //set output paramters
                for (var i=0; i< parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    if (parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef)
                    {
                        if (callParams.ContainsKey(parameterInfo.Name))
                            ret.OutParams[parameterInfo.Name] = parameterValues[i];
                    }
                }

                return Task.FromResult(ret);
            }
        }

        public Task<SpInfo[]> GetSystemApi(out string context)
        {
            var ctx = new InvokeContext(appName: _type.Name, authUserId: "$$", audience: null);
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

        private SpInfo ProcInfo_FromMethodInfo(MethodInfo methodInfo)
        {
            var paramsEx = new Dictionary<string, SpParamInfoEx>();

            //create paramInfo
            var spParamInfos = new List<SpParamInfo>();
            foreach (var paramInfo in methodInfo.GetParameters())
            {
                var spParamInfo = new SpParamInfo
                {
                    ParamName = paramInfo.Name,
                    IsOutput = paramInfo.IsOut || paramInfo.IsRetval,
                    IsOptional = paramInfo.IsOptional,
                    DefaultValue = paramInfo.DefaultValue,
                    UserTypeName = paramInfo.ParameterType.Name,
                    SystemTypeName = paramInfo.ParameterType.Name
                };
                spParamInfos.Add(spParamInfo);

                //extended paramInfo
                var paramAttribute = paramInfo.GetCustomAttribute<DirectSpParamAttribute>() ?? new DirectSpParamAttribute();
                var spParamInfoEx = new SpParamInfoEx()
                {
                    SignType = paramAttribute.SignType
                };

                paramsEx[paramInfo.Name] = spParamInfoEx;
            }

            var directSpAttribute = methodInfo.GetCustomAttribute<DirectSpProcAttribute>() ?? new DirectSpProcAttribute();

            //create procedure infos
            var spInfo = new SpInfo
            {
                ProcedureName = methodInfo.Name,
                Params = spParamInfos.ToArray(),
                ExtendedProps = new SpInfoEx()
                {
                    IsBatchAllowed = directSpAttribute.IsBatchAllowed,
                    Params = paramsEx
                }
            };

            return spInfo;
        }
    }
}
