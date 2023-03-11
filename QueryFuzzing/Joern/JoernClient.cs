using QueryFuzzing.Joern.Models;
using System.Text.Json;

namespace QueryFuzzing.Joern
{
    public class JoernClient : IJoernClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public JoernClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResultResponse> RetrieveResponse(string uuid)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("joernServer");

                var response = await client.GetAsync($"result/{uuid}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<ResultResponse>(content);
                }
                throw new HttpRequestException();

            }
            catch (Exception ex) 
            {
                throw(ex);
            }
        }

        public async Task<QueryResponse> SubmitQuery(QueryRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("joernServer");

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                var content = new StringContent(JsonSerializer.Serialize(request));

                var response = await client.PostAsync("query", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<QueryResponse>(result);
                }
                throw new HttpRequestException();
            }
            catch (Exception ex)
            { 
                throw(ex);
            }
        }
    }
}
