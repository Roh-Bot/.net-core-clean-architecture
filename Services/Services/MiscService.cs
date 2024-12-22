using Core.ServiceContracts;

namespace Core.Services
{
    public class MiscService(IHttpClientFactory httpClientFactory) : IMiscService
    {
        public async Task<string> GetCatFacts()
        {
            using var httpClient = httpClientFactory.CreateClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://cat-fact.herokuapp.com/facts/random"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { "Content-Type", "application/json" }
                }
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cts.Token);
        }
    }
}
