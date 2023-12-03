namespace QueryFuzzingWebApp.Database.Models
{
    public class Stacktrace
    {
        public int Id { get; set; }
        public bool GlobalLib { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
        public int LineNumber { get; set; }
        public int CrashId { get; set; }
        public Crash Crash { get; set; }
    }
}
