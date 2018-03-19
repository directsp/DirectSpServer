using Newtonsoft.Json;
using System;

namespace DirectSpLib
{
    public class AntiXssConverter : JsonConverter
    {
        private bool IsValidate;

        public AntiXssConverter(bool isValidate = false)
        {
            IsValidate = isValidate;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (IsValidate)
                Util.AntiXss_VerifyValue((string)value, true);
            else
                value = Util.AntiXss_Encode((string)value);

            writer.WriteValue(Util.AntiXss_Encode((string)value));

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;
    }
}