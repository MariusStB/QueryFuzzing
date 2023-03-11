using System.Text.Json.Serialization;

namespace QueryFuzzing.Joern.Models
{
    public class QueryRequest
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }
    }
}
