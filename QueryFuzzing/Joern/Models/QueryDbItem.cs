

using System.Text.Json.Serialization;

namespace QueryFuzzing.Joern.Models
{
    public class QueryDbItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("score")]
        public float Score { get; set; }
        [JsonPropertyName("traversal")]
        public Traversal Traversal { get; set; }
        [JsonPropertyName("traversalAsString")]
        public string TraversalAsString { get; set; }
        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; }
        [JsonPropertyName("codeExamples")]
        public Codeexamples CodeExamples { get; set; }
        [JsonPropertyName("multiFileCodeExamples")]
        public Multifilecodeexamples MultiFileCodeExamples { get; set; }
    }   
        public class Traversal { }
    public class Codeexamples
    {
        [JsonPropertyName("positive")]
        public string[] Positive { get; set; }
        [JsonPropertyName("negative")]
        public string[] Negative { get; set; }
    }

    public class Multifilecodeexamples
    {
        [JsonPropertyName("positive")]
        public object[] Positive { get; set; }
        [JsonPropertyName("negative")]
        public object[] Negative { get; set; }
    }

}
