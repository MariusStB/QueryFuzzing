using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
