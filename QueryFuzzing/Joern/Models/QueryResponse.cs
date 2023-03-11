using System.Text.Json.Serialization;

namespace QueryFuzzing.Joern.Models
{
    public class QueryResponse
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}
