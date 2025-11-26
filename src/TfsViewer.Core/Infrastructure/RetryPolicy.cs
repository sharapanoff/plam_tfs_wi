using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Net.Http;
using Microsoft.VisualStudio.Services.Common;
using TfsViewer.Core.Services;

namespace TfsViewer.Core.Infrastructure;

public static class RetryPolicy
{
    public static AsyncRetryPolicy CreateTfsDefaultPolicy(ILoggingService? logging = null)
    {
        // 3 retries, exponential backoff jittered
        var delays = Backoff.ExponentialBackoff(TimeSpan.FromSeconds(1), retryCount: 3);

        return Policy
            .Handle<HttpRequestException>()
            .Or<VssServiceException>()
            .WaitAndRetryAsync(delays, (exception, sleep, attempt, context) =>
            {
                logging?.LogWarning($"Retry attempt {attempt} after {sleep.TotalMilliseconds}ms due to: {exception.Message}");
            });
    }
}
