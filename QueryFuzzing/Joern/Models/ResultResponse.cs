using System.Text.Json.Serialization;

namespace QueryFuzzing.Joern.Models
{
    public class ResultResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("stdout")]
        public string Stdout { get; set; }
        [JsonPropertyName("stderr")]
        public string Stderr { get; set; }
    }
}
