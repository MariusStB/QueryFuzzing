using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFuzzing.Valgrind.Models
{
    public class TraceEquals :IEqualityComparer<Trace>
    {
        public bool Equals(Trace x, Trace y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return x.ClassName == y.ClassName && x.Method == y.Method && x.LineNumber==y.LineNumber;

        }

        public int GetHashCode(Trace trace)
        {
            return (trace.ClassName + trace.Method + trace.LineNumber).GetHashCode();
        }
    }
}
