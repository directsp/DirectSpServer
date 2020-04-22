
using System.Text.Json.Serialization;

namespace DirectSp
{
    public class SpCallError
    {
        [JsonPropertyName("errorType")]
        public string ErrorType { get; set; }
        
        [JsonPropertyName("errorNumber")]
        public int ErrorNumber { get; set; }


        [JsonPropertyName("errorName")]
        public string ErrorName { get; set; }
        
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
        
        [JsonPropertyName("errorDescription")]
        public string ErrorDescription { get; set; }
        
        [JsonPropertyName("errorProcName")]
        public object ErrorProcName { get; set; }
        
        [JsonPropertyName("errorData")]
        public object ErrorData { get; set; }
    }
}
