using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFuzzing.Windranger
{
    public class WindrangerService : IWindrangerService
    {
        public Task<bool> CleanUp()
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetResults()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> PrepareFuzzing(string projectPath)
        {
            DockerExecuter.ExecDockerCommand($"cp -r {projectPath} fuzz:/home/SVF-tools/example");

            return true;
        }

        public async Task<bool> StartFuzzing()
        {
            DockerExecuter.ExecDockerCommand("exec -it fuzz bash -c \"/home/SVF-tools/example/fuzzing.sh\"");
            return true;
        }

        private static void CreateFuzzingScript(string projectPath)
        {
            var sb = new StringBuilder();
            sb.Append($"./autogen.sh{Environment.NewLine}");
            sb.Append($"mkdir build; cd build{Environment.NewLine}");
            sb.Append($"CC=~/gllvm/gclang CXX=~/gllvm/gclang++ ../configure --disable-shared{Environment.NewLine}");
            sb.Append($"make clean; make{Environment.NewLine}");
            sb.Append($"cd util; ~/gllvm/get-bc swftophp{Environment.NewLine}");
            
        }
    }
}
