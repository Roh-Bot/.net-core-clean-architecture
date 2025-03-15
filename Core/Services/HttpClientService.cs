using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Core.ServiceRecords;
using Microsoft.IdentityModel.Tokens;

namespace Core.Services
{
    public class HttpClientService(HttpClient httpClient, IConfiguration configuration, ILogger<HttpClientService> logger) : IHttpClientService
    {
        private readonly int _retryAfter = Convert.ToInt32(configuration["Http:RetryCount"]);
        private readonly int _retryCount = Convert.ToInt32(configuration["Http:RetryCount"]);
        public async Task<HttpResponseMessage> SendAsync(HttpRequestRecord request, CancellationToken cancellationToken)
        {
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult(response =>
                    response.StatusCode
                        is HttpStatusCode.InternalServerError
                        or HttpStatusCode.GatewayTimeout
                        or HttpStatusCode.ServiceUnavailable
                        or HttpStatusCode.NotFound
                )
                .WaitAndRetryAsync(
                    _retryCount,
                    _ => TimeSpan.FromSeconds(_retryAfter),
                    onRetry: (response, timespan, retryCount, _) =>
                    {
                        logger.LogWarning(
                            "Retrying {RetryCount}/{MaxRetries} after {Delay} due to {Reason}",
                            retryCount,
                            _retryCount,
                            timespan,
                            response.Exception?.Message ?? response.Result?.StatusCode.ToString()
                        );
                    }
                );

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(20), TimeoutStrategy.Optimistic);

            return await retryPolicy.WrapAsync(timeoutPolicy).ExecuteAsync(async () =>
            {
                // Creating newRequest avoid System.InvalidOperationException for reusing the same request.
                using var newRequest = new HttpRequestMessage()
                {
                    Content = JsonContent.Create(request.Body),
                    Method = request.Method,
                    RequestUri = request.RequestUri,
                };

                if (!request.Headers.IsNullOrEmpty())
                {
                    foreach (var header in request.Headers)
                    {
                        newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                logger.LogInformation("Attempting HTTP {Method} request to {Url}", newRequest.Method, newRequest.RequestUri);

                var response = await httpClient.SendAsync(request: newRequest, cancellationToken: cancellationToken);

                var responseBody = string.Empty;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    responseBody = await new StreamReader(memoryStream).ReadToEndAsync(cancellationToken);

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    response.Content = new StreamContent(memoryStream);
                }

                logger.LogInformation("Received HTTP response with status code {StatusCode} for {Url} {ResponseBody}", response.StatusCode, newRequest.RequestUri, responseBody);
                return response;
            });
        }
    }
}
