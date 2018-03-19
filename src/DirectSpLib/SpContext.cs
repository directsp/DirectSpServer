using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DirectSpLib.Entities;
using System;

namespace DirectSpLib
{
    public class SpContext
    {
        private JObject Body;
        public DateTime? ModifiedTime { get; private set; }
        public string AppName { get; private set; }
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
            //create new invokeOptions
            dynamic obj = Body;

            obj.InvokeOptions = new JObject();
            if (spCallOptions.IsBatch) obj.InvokeOptions.IsBatch = spCallOptions.IsBatch;
            if (spCallOptions.IsCaptcha) obj.InvokeOptions.IsCaptcha = spCallOptions.IsCaptcha;
            if (spCallOptions.MoneyConversionRate != 1) obj.InvokeOptions.MoneyConversionRate = spCallOptions.MoneyConversionRate;
            if (spCallOptions.RecordIndex != null) obj.InvokeOptions.RecordIndex = spCallOptions.RecordIndex;
            if (spCallOptions.RecordCount != null) obj.InvokeOptions.RecordCount = spCallOptions.RecordCount;

            return JsonConvert.SerializeObject(obj); //ToString will add linefeed
        }
    }
}
