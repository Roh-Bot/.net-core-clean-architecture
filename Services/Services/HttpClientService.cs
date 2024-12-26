using Core.ServiceContracts;

namespace Core.Services
{
    public class HttpClientService(HttpClient httpClient) : IHttpClientService
    {
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            return await httpClient.SendAsync(request, httpCompletionOption, cancellationToken);
        }
    }
}
