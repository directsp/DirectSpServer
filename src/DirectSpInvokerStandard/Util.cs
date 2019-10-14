using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;

namespace DirectSp
{
    public static class Util
    {
        public static T SafeDBNull<T>(object value, T defaultValue = default)
        {
            if (value == null)
                return default;

            if (value is string)
                return (T)Convert.ChangeType(value, typeof(T));

            return (value == DBNull.Value) ? defaultValue : (T)value;
        }

        public static JToken CamelizeJToken(JToken jToken)
        {
            return RenameJToken(jToken, true);
        }

        public static JToken PascalizeJToken(JToken jToken)
        {
            return RenameJToken(jToken, false);
        }

        private static JToken RenameJToken(JToken jToken, bool camelize)
        {
            if (jToken == null)
                return jToken;

            //enum json array
            if (jToken.Type == JTokenType.Array)
            {
                foreach (var item in jToken)
                    RenameJToken(item, camelize);
            }

            //enum json properties
            else if (jToken.Type == JTokenType.Object)
            {
                var jObject = jToken as JObject;
                foreach (var jp in jObject.Properties().ToList())
                {
                    RenameJToken(jp.Value, camelize);
                    jp.Replace(new JProperty(camelize ? StringHelper.ToCamelCase(jp.Name) : StringHelper.ToPascalCase(jp.Name), jp.Value));
                }
            }

            return jToken;
        }

        public static string GetClaimUserId(ClaimsPrincipal user)
        {
            return user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        }

        public static string GetClaimAudience(ClaimsPrincipal user)
        {
            var ret = user.Claims.FirstOrDefault(x => x.Type == "aud");
            return ret?.Value;
        }

        public static string GetFriendlySqlTypeName(string providerSpecificFieldTypeName)
        {
            providerSpecificFieldTypeName = providerSpecificFieldTypeName.ToLower();

            if (providerSpecificFieldTypeName == "timespan") return "timespan";
            else if (providerSpecificFieldTypeName.IndexOf("char") != -1) return "string";
            else if (providerSpecificFieldTypeName.IndexOf("date") != -1) return "datetime";
            else if (providerSpecificFieldTypeName.IndexOf("time") != -1) return "datetime";
            else if (providerSpecificFieldTypeName.IndexOf("money") != -1) return "money";
            else if (providerSpecificFieldTypeName.IndexOf("int") != -1) return "integer";
            else if (providerSpecificFieldTypeName.IndexOf("bit") != -1 ) return "boolean";
            else if (providerSpecificFieldTypeName.IndexOf("float") != -1 || providerSpecificFieldTypeName.IndexOf("decimal") != -1) return "float";
            return "string";
        }

        public static bool IsJsonString(string str)
        {
            if (str == null)
                return false;

            str = str.Trim();
            return
                (str.StartsWith("{") && str.EndsWith("}")) || //For object
                (str.StartsWith("[") && str.EndsWith("]"));

        }

        public static string ToJsonString(object value, bool camelize)
        {
            var settings = new JsonSerializerSettings();
            if (camelize)
                settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(value, settings);
        }

        public static string GetRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            var ret = new string(stringChars);
            return ret;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly double MaxUnixSeconds = (DateTime.MaxValue - UnixEpoch).TotalSeconds;
        public static double DateTime_ToUnixDate(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - UnixEpoch).TotalSeconds;
        }

        public static DateTime DateTime_FromUnixDate(double unixDate)
        {

            return unixDate > MaxUnixSeconds
                ? UnixEpoch.AddMilliseconds(unixDate)
                : UnixEpoch.AddSeconds(unixDate);
        }

    }
}
