using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace DirectSp.Client.Entities
{
    public class AuthTokens
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }

        public JObject parseAccessToken()
        {
            var base64Url = access_token.Split('.')[1];
            var base64 = base64Url.Replace('-', '+').Replace('_', '/');
            if (base64.Length % 4 > 0)
                base64 = base64.PadRight(base64.Length + 4 - base64.Length % 4, '=');
            var data = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(data);
            return JObject.Parse(json);
        }
    }
}
