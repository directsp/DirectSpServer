using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DirectSp.Entities;
using System;

namespace DirectSp
{
    public class DirectSpInvokeContext
    {
        private JObject Body;
        public DateTime? ModifiedTime { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public string AuthUserId { get; private set; }

        public DirectSpInvokeContext(string body, string authUserId = null)
        {
            Construct(body, authUserId);
        }

        public DirectSpInvokeContext(string appName, string authUserId, string audience)
        {
            var obj = new JObject
            {
                ["AppName"] = appName,
                ["Audience"] = audience,
                ["AuthUserId"] = authUserId
            };
            Construct(obj.ToString());
        }

        private void Construct(string body, string authUserId = null)
        {
            JToken jToken;
            AuthUserId = authUserId;

            var obj = (JObject)JsonConvert.DeserializeObject(body);
            if (obj.TryGetValue("ModifiedTime", out jToken)) ModifiedTime = (DateTime?)jToken;
            if (obj.TryGetValue("AppVersion", out jToken)) AppVersion = (string)jToken;
            if (obj.TryGetValue("AppName", out jToken)) AppName = (string)jToken;
            if (authUserId==null && obj.TryGetValue("AuthUserId", out jToken)) AuthUserId = (string)jToken;

            obj["InvokeOptions"] = null; //remove InvokeOptions
            Body = obj;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Body); //ToString will add linefeed
        }

        public string ToString(SpCallOptions spCallOptions)
        {
            var serializeSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
            
            //create new invokeOptions
            var obj = Body.DeepClone();
            obj["InvokeOptions"] = JToken.FromObject(spCallOptions); //add options
            var ret = JsonConvert.SerializeObject(obj);
            return ret;
        }
    }
}
