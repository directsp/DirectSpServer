using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DirectSp
{
   [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpCallError
    {
        public string ErrorType { get; set; }
        public int ErrorId { get; set; }
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDescription { get; set; }
        public object ErrorProcName { get; set; }
        public object ErrorData { get; set; }
    }
}
