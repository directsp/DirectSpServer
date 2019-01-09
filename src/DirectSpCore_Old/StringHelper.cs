using System;
using System.Text;

namespace DirectSp.Core
{
    public static class StringHelper
    {
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
                if (char.IsLower(str[i]) && i > 1)
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

            var ret = char.ToLowerInvariant(str[0]) + str.Substring(1);
            return ret;
        }

        public static string ToPascalCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Substring(0, 1).ToUpper() + str.Substring(1);
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

        public static string ToBase64(string str)
        {
            return ToBase64(str, Encoding.UTF8);
        }

        private static string ToBase64(string str, Encoding encoding)
        {
            var textAsBytes = encoding.GetBytes(str);
            return Convert.ToBase64String(textAsBytes);
        }

        public static string FromBase64(string str)
        {
            return FromBase64(str, Encoding.UTF8);
        }
        public static string FromBase64(string text, Encoding encoding)
        {
            var textAsBytes = Convert.FromBase64String(text);
            return encoding.GetString(textAsBytes);
        }
    }
}
