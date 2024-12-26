using Core.ServiceContracts;

namespace Core.Services
{
    public class MiscService(IHttpClientService httpClient) : IMiscService
    {
        public async Task<string> GetCatFacts()
        {
            var response = await httpClient.SendAsync(new HttpRequestMessage(),
                HttpCompletionOption.ResponseContentRead, new CancellationTokenSource().Token);
            return string.Empty;
        }
    }
}
