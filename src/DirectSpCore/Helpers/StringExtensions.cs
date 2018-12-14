using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Helpers
{
    public static class StringExtensions
    {
        public static string ToBase64(this string str)
        {
            return ToBase64(str, Encoding.UTF8);
        }

        private static string ToBase64(this string str, Encoding encoding)
        {
            var textAsBytes = encoding.GetBytes(str);
            return Convert.ToBase64String(textAsBytes);
        }

        public static string FromBase64(this string str)
        {
            return FromBase64(str, Encoding.UTF8);
        }
        public static string FromBase64(this string text, Encoding encoding)
        {
            var textAsBytes = Convert.FromBase64String(text);
            return encoding.GetString(textAsBytes);
        }
    }
}
