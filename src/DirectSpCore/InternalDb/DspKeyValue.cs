using Newtonsoft.Json;
using DirectSp.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace DirectSp.Core.InternalDb
{
    public class DspKeyValue
    {
        public SpInvoker SpInvoker { get; private set; }

        public DspKeyValue(SpInvoker spInvoker)
        {
            SpInvoker = spInvoker;
        }

        public async Task<IEnumerable<DspKeyValueItem>> All(string keyNamePattern = null)
        {
            var spInvokeParams = new SpInvokeParams();

            var spCall = new SpCall() { Method = "KeyValue_" + nameof(All) };
            spCall.Params.Add("KeyNamePattern", keyNamePattern);

            var ret = await SpInvoker.Invoke(spCall, spInvokeParams, true);
            return JsonConvert.DeserializeObject<List<DspKeyValueItem>>(JsonConvert.SerializeObject(ret.Recordset));
        }

        public async Task<SpCallResult> ValueSet(string keyName, string textValue, int timeToLife = 0, bool isOverwrite = true)
        {
            var spInvokeParams = new SpInvokeParams();

            var spCall = new SpCall() { Method = "KeyValue_" + nameof(ValueSet) };
            spCall.Params.Add("KeyName", keyName);
            spCall.Params.Add("TextValue", textValue);
            spCall.Params.Add("TimeToLife", timeToLife);
            spCall.Params.Add("IsOverwrite", isOverwrite);

            return await SpInvoker.Invoke(spCall, spInvokeParams, true);
        }

        public async Task<object> Value(string keyName)
        {
            var spInvokeParams = new SpInvokeParams();

            var spCall = new SpCall() { Method = "KeyValue_" + nameof(Value) };
            spCall.Params.Add("KeyName", keyName);

            var res = await SpInvoker.Invoke(spCall, spInvokeParams, true);
            return res["TextValue"];
        }

        public async Task<Stream> GetTextStream(string keyName, Encoding encoding)
        {
            var text = await Value(keyName) as string;

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, encoding);
            sw.Write(text);
            sw.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}