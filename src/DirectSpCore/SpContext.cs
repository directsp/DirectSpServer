using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DirectSp.Core.Entities;
using System;

namespace DirectSp.Core
{
    public class SpContext
    {
        private JObject Body;
        public DateTime? ModifiedTime { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public string AuthUserId { get; private set; }

        public SpContext(string body, string authUserId = null)
        {
            Construct(body, authUserId);
        }


        public SpContext(string appName, string authUserId, string audience)
        {
            dynamic obj = new JObject();
            obj.AppName = appName;
            obj.Audience = audience;
            obj.AuthUserId = authUserId;

            Construct(obj.ToString());
        }

        private void Construct(string body, string authUserId = null)
        {
            dynamic obj = JsonConvert.DeserializeObject(body);
            ModifiedTime = obj.ModifiedTime;
            AppVersion = obj.AppVersion;
            AppName = obj.AppName;
            AuthUserId = authUserId ?? obj.AuthUserId;
            obj.InvokeOptions = null; //remove InvokeOptions
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
            dynamic obj = Body.DeepClone();
            obj.InvokeOptions = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(spCallOptions, serializeSettings)); //add options
            var ret = JsonConvert.SerializeObject(obj);
            return ret;
        }
    }
}
