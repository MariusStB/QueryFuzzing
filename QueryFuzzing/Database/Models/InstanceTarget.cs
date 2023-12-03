namespace QueryFuzzingWebApp.Database.Models
{
    public class InstanceTarget:Target
    {
        public bool Crashed { get; set; }
        public int FuzzingInstanceId { get; set; }
        public FuzzingInstance FuzzingInstance { get; set; }
    }
}
