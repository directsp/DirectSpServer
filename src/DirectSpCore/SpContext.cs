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
        public string UserId { get; private set; }

        public SpContext(string body)
        {
            Construct(body);
        }

        public SpContext(string appName, string userId, string audience)
        {
            dynamic obj = new JObject();
            obj.AppName = appName;
            obj.Audience = audience;
            obj.User = new JObject();
            obj.User.AuthUserId = userId;

            Construct(obj.ToString());
        }

        private void Construct(string body)
        {
            dynamic obj = JsonConvert.DeserializeObject(body);
            ModifiedTime = obj.ModifiedTime;
            AppVersion = obj.AppVersion;
            AppName = obj.AppName;
            UserId = obj.User.AuthUserId;
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
