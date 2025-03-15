using Core.ServiceContracts;
using Core.ServiceRecords;

namespace Core.Services
{
    public class MiscService(IHttpClientService httpClient) : IMiscService
    {
        public async Task<string> GetCatFacts()
        {
            var request = new HttpRequestRecord
            {
                RequestUri = new Uri("https://cat-fact.herokuapp.com/facts"),
                Method = HttpMethod.Get,
                Headers = {{"Content-Type","application/json"}},
                Body = ""
            };
            var response = await httpClient.SendAsync(request, new CancellationTokenSource().Token);
            return string.Empty;
        }
    }
}
