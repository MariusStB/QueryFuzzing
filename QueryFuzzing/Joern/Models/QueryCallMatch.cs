
using System.Text.Json.Serialization;

namespace QueryFuzzing.Joern.Models
{
    public class QueryCallMatch
    {
        [JsonPropertyName("_1")]
        public int LineNumber { get; set; }
        [JsonPropertyName("_2")]
        public string Path { get; set; }
        [JsonPropertyName("_3")]
        public string Methodname { get; set; }
    }
}
