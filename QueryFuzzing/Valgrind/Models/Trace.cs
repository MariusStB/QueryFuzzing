
namespace QueryFuzzing.Valgrind.Models
{
    public class Trace
    {
        public int Id { get; set; }
        public bool GlobalLib { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
        public int LineNumber { get; set; }
        
    }
}
