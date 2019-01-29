using System;
using System.Globalization;

namespace DirectSp
{
    internal class AlternativeCalendar
    {
        public CultureInfo AlternativeCulture { get; }

        public AlternativeCalendar(CultureInfo alternativeCulture)
        {
            AlternativeCulture = alternativeCulture;
        }

        public bool IsDateTime(string typeName)
        {
            return typeName.Length >=4 && typeName.ToLower().Substring(0, 4) == "date" && AlternativeCulture != null;
        }

        public string FormatDateTime(object fieldValue, string typeName)
        {
            return fieldValue == null ? null : ((DateTime)fieldValue).ToString(typeName.ToLower() == "date" ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss", AlternativeCulture);
        }

        public string GetFieldName(string fieldName)
        {
            return $"{fieldName}_{AlternativeCulture.TwoLetterISOLanguageName}";
        }

    }
}
