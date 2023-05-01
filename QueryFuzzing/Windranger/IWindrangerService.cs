using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFuzzing.Windranger
{
    public interface IWindrangerService
    {
        Task<bool> PrepareFuzzing(string projectPath);
        Task<bool> StartFuzzing();
        Task<bool> GetResults();
        Task<bool> CleanUp();
    }
}
