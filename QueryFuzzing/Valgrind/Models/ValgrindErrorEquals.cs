using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFuzzing.Valgrind.Models
{
    public class ValgrindErrorEquals : IEqualityComparer<ValgrindError>
    {
        public bool Equals(ValgrindError x, ValgrindError y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            var traceEquals = new TraceEquals();
            return x.ErrorMessage == y.ErrorMessage && x.FuzzingCrashTime == y.FuzzingCrashTime && y.Stacktrace.SequenceEqual(x.Stacktrace, traceEquals);

        }

        public int GetHashCode(ValgrindError valgrindError)
        {
            return (valgrindError.ErrorMessage + valgrindError.FuzzingCrashTime+valgrindError.Stacktrace).GetHashCode();
        }
    }
}
