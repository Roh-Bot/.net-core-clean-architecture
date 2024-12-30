using System.Net;
using Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace FundApi.Core.Services
{
    public class HttpClientService(HttpClient httpClient, IConfiguration configuration, ILogger<HttpClientService> logger) : IHttpClientService
    {
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult(response =>
                    response.StatusCode
                        is HttpStatusCode.InternalServerError
                        or HttpStatusCode.GatewayTimeout
                        or HttpStatusCode.ServiceUnavailable
                )
                .WaitAndRetryAsync(
                    Convert.ToInt32(configuration["Http:RetryCount"]!),
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (response, timespan, retryCount, _) =>
                    {
                        logger.LogWarning(
                            "Retrying {RetryCount}/{MaxRetries} after {Delay} due to {Reason}",
                            retryCount,
                            configuration["Http:RetryCount"],
                            timespan,
                            response.Exception?.Message ?? response.Result?.StatusCode.ToString()
                        );
                    }
                );

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(20), TimeoutStrategy.Optimistic);

            return await retryPolicy.WrapAsync(timeoutPolicy).ExecuteAsync(async () =>
                await httpClient.SendAsync(request, httpCompletionOption, cancellationToken)
            );
        }
    }
}
