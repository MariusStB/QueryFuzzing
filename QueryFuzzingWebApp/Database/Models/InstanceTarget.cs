namespace QueryFuzzingWebApp.Database.Models
{
    public class InstanceTarget:Target
    {
        public int FuzzingInstanceId { get; set; }
        public FuzzingInstance FuzzingInstance { get; set; }
    }
}
