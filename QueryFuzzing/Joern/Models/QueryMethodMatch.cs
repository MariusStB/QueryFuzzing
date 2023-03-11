

namespace QueryFuzzing.Joern.Models
{
    public class QueryMethodMatch
    {
        public string Id { get; set; }
        public string AstParentFullName { get; set; }
        public string AstParentType { get; set; }
        public string Code { get; set; }
        public int ColumnNumber { get; set; }
        public int ColumnNumberEnd { get; set; }
        public string Filename { get; set; }
        public string FullName{ get; set; }
        public string Hash { get; set; }
        public string IsExternal { get; set; }
        public int LineNumber { get; set; }
        public int LineNumberEnd { get; set; }
        public string Name { get; set; }
        public string Order { get; set; }
        public string Signature { get; set; }
    }
}
