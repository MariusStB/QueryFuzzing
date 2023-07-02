using QueryFuzzing.Joern.Models;
using System.Text.Json;

namespace QueryFuzzing.Joern
{
    public class JoernService : IJoernService
    {
        private readonly IJoernClient _joernClient;

        public JoernService(IJoernClient joernClient)
        {
            _joernClient = joernClient;
        }

        public async Task<List<string>> GetQueryDbItems(Language language, Tag tag)
        {
            try
            {
                using FileStream stream = File.OpenRead("Ressources//querydb.json");
                var querydbItems = await JsonSerializer.DeserializeAsync<List<QueryDbItem>>(stream);
                
                if(language!= null)
                {
                    querydbItems = querydbItems.Where(q => q.Language == language.ToString()).ToList();

                }

                if(tag != Tag.none)
                {
                    querydbItems = querydbItems.Where(q => q.Tags.Contains(tag.ToString())).ToList();
                }

                var queryList = querydbItems.Select(q => $"({{{q.TraversalAsString.Replace("cpg =>\n", "").Trim()}}}).map(c => (c.lineNumber.get, c.location.filename, c.location.methodFullName)).toJson").ToList();
                //var queryList = querydbItems.Select(q => $"({{{q.TraversalAsString.Replace("cpg =>\n", "").Trim()}}}).l").ToList();
                return queryList;
            }catch(Exception ex)
            {
                return new List<string>();
            }
            
        }

        public async Task<bool> ImportProject(string projectName, string projectPath)
        {
            try
            {
#if(false)
                return true;
#endif
                var queryResponse = await _joernClient.SubmitQuery(new Models.QueryRequest { 
                    Query = $"importCode(inputPath=\"{projectPath}\", projectName=\"{projectName}\")" 
                });

                var resultResponse = await _joernClient.RetrieveResponse(queryResponse.Uuid);
                while (!resultResponse.Success)
                {
                    Thread.Sleep(100);
                    resultResponse = await _joernClient.RetrieveResponse(queryResponse.Uuid);
                }
                Console.WriteLine($"Success: {resultResponse.Success}");
                Console.WriteLine($"Stderr: {resultResponse.Stderr}");
                Console.WriteLine($"Stdout: {resultResponse.Stdout}");
                Console.WriteLine("----------------------------------------");

                return resultResponse.Success;
            }
            catch(Exception ex) {
                return false;
            }
        }

        public async Task<List<string>> SendQuery(string query)
        {
            try
            {
                Console.WriteLine(query);
                var queryResponse = await _joernClient.SubmitQuery(new Models.QueryRequest
                {
                    Query = query
                });

                ResultResponse resultResponse = await _joernClient.RetrieveResponse(queryResponse.Uuid);
                while (!resultResponse.Success) {
                    Thread.Sleep(100);
                    resultResponse = await _joernClient.RetrieveResponse(queryResponse.Uuid);
                }
                Console.WriteLine($"Success: {resultResponse.Success}");
                Console.WriteLine($"Stderr: {resultResponse.Stderr}");
                Console.WriteLine($"Stdout: {resultResponse.Stdout}");
                Console.WriteLine("----------------------------------------");
                return new List<string> { resultResponse.Stdout };

            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
