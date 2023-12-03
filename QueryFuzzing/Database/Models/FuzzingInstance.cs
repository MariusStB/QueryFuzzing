namespace QueryFuzzingWebApp.Database.Models
{
    public class FuzzingInstance
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Executable SelectedExecutable { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int FinalStatId { get; set; }
        public FuzzingStat FinalStats {get ;set;}
        public ICollection<Crash> Crashes { get; set; }
        public ICollection<Executable> Executables { get; set; } = new List<Executable>();
        public ICollection<InstanceTarget> InstanceTargets { get; set; } = new List<InstanceTarget>();
    }
}
