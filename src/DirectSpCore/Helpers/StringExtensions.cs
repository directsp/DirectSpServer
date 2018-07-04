using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Helpers
{
    public static class StringExtensions
    {
        public static string ToBase64(this string text)
        {
            return ToBase64(text, Encoding.UTF8);
        }

        private static string ToBase64(this string text, Encoding encoding)
        {
            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static string FromBase64(this string text)
        {
            return FromBase64(text, Encoding.UTF8);
        }
        public static string FromBase64(this string text, Encoding encoding)
        {
            byte[] textAsBytes = Convert.FromBase64String(text);
            return encoding.GetString(textAsBytes);
        }
    }
}
