using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Project.Infrastructure.Clients;

public class HttpPollyClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<HttpPollyClient> logger)
{
    public async Task<HttpResponseMessage> SendAsync(HttpExternalClients client, HttpRequestRecord request, CancellationToken cancellationToken)
    {
        using var newRequest = new HttpRequestMessage()
        {
            Content = JsonContent.Create(request.Body),
            Method = request.Method,
            RequestUri = request.RequestUri,
        };

        if (request.Headers.Any())
        {
            foreach (var header in request.Headers)
            {
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        logger.LogInformation("Attempting HTTP {Method} request to {Url}", newRequest.Method, newRequest.RequestUri);

        var httpClient = httpClientFactory.CreateClient(client.ToString());
        var response = await httpClient.SendAsync(newRequest, cancellationToken);

        logger.LogInformation("Received HTTP response with status code {StatusCode} for {Url}", response.StatusCode, newRequest.RequestUri);
        return response;
    }
}
public record HttpRequestRecord
{
    public required Uri RequestUri { get; init; }
    public required HttpMethod Method { get; init; }
    public HttpRequestHeaders Headers { get; init; } = new HttpRequestMessage().Headers;
    public object? Body { get; init; }
}

public enum HttpExternalClients
{
    Default,
    Weather
}