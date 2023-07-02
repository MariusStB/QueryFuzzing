using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFuzzing.Valgrind.Models
{
    public class ValgrindError
    {
        public string ErrorMessage { get; set; }
        public TimeSpan FuzzingCrashTime { get; set; }
        public List<Trace> Stacktrace { get; set; }

    }
}
