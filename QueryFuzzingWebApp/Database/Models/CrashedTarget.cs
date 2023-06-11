namespace QueryFuzzingWebApp.Database.Models
{
    public class CrashedTarget : Target
    {
        public int Column { get; set; }

        public int CrashId { get; set; }
        public Crash Crash { get; set; }
    }
}
