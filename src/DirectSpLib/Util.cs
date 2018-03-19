using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DirectSpLib
{
    public static class Util
    {
        public static T SafeDBNull<T>(object value, T defaultValue = default(T))
        {
            if (value == null)
                return default(T);

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
                    jp.Replace(new JProperty(camelize ? ToCamelCase(jp.Name) : ToPascalCase(jp.Name), jp.Value));
                }
            }

            return jToken;
        }

        public static string ToCamelCase(string str)
        {
            return ToCamelCaseSimple(str);
        }


        private static string ToCamelCaseWise(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var ret = str.ToLower();
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsLower(str[i]) && i > 1)
                {
                    ret = str.Substring(0, i - 1).ToLower() + str.Substring(i - 1);
                    break;
                }
            }

            return ret;
        }

        private static string ToCamelCaseSimple(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var ret = Char.ToLowerInvariant(str[0]) + str.Substring(1);
            return ret;
        }

        public static string ToPascalCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Substring(0, 1).ToUpper() + str.Substring(1);
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
            else if (providerSpecificFieldTypeName.IndexOf("float") != -1 || providerSpecificFieldTypeName.IndexOf("decimal") != -1) return "float";
            else if (providerSpecificFieldTypeName.IndexOf("bit") != -1 || providerSpecificFieldTypeName.IndexOf("decimal") != -1) return "boolean";
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

            var ret = new String(stringChars);
            return ret;
        }


        public static bool AntiXss_VerifyValue(string text, bool isThrow)
        {
            if (text == null)
                return true;

            char[] invalidCharacters = { '"', '\'', '<', '>', '&' };
            var res = text.IndexOfAny(invalidCharacters) == -1;
            if (!res && isThrow)
                throw new ArgumentException("the argument contains invalid characters! \" ' < > &");
            return res;
        }

        public static string AntiXss_Encode(string text)
        {
            if (text == null)
                return null;

            //text = text.Replace("<", "&lt;");
            //text = text.Replace(">", "&gt;");
            //text = text.Replace("\"", "&quot;"); 
            //text = text.Replace("javascript", "java-script", StringComparison.InvariantCultureIgnoreCase);
            text = System.Web.HttpUtility.HtmlDecode(text);
            text = System.Web.HttpUtility.HtmlEncode(text);
            return text;
        }

        public static string FixUserString(string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
                return null;

            var str = new StringBuilder(value.Length);
            var lastItem = '\0';
            foreach (var item in value)
            {
                switch (item)
                {
                    case '٠': str.Append('0'); break;
                    case '١': str.Append('1'); break;
                    case '٢': str.Append('2'); break;
                    case '٣': str.Append('3'); break;
                    case '٤': str.Append('4'); break;
                    case '٥': str.Append('5'); break;
                    case '٦': str.Append('6'); break;
                    case '٧': str.Append('7'); break;
                    case '٨': str.Append('8'); break;
                    case '٩': str.Append('9'); break;

                    case '۰': str.Append('0'); break;
                    case '۱': str.Append('1'); break;
                    case '۲': str.Append('2'); break;
                    case '۳': str.Append('3'); break;
                    case '۴': str.Append('4'); break;
                    case '۵': str.Append('5'); break;
                    case '۶': str.Append('6'); break;
                    case '۷': str.Append('7'); break;
                    case '۸': str.Append('8'); break;
                    case '۹': str.Append('9'); break;

                    case 'ي': str.Append('ی'); break;
                    case 'ك': str.Append('ک'); break;
                    case ' ': if (lastItem != ' ') str.Append(item); break;
                    default: str.Append(item); break;
                }
                lastItem = item;
            }

            return str.ToString();
        }
    }
}
