using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Net;
using System.Runtime.Caching;

/// <summary>
/// A simple HTTP Client with resilience and caching.
/// </summary>
public class SimpleHttpClient
{
    private static readonly MemoryCache cache = MemoryCache.Default;

    /// <summary>
    /// Gets the response of the WebService.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <returns>The response of the WebService</returns>
    /// <exception cref="SimpleHttpClientException"></exception>
    public async Task<HttpResponseMessage> Get(Uri url)
    {
        return await CacheContent(url);
    }

    /// <summary>
    /// Caches the response of the WebService for 1 minute.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <returns>The response of the WebService</returns>
    /// <exception cref="SimpleHttpClientException"></exception>
    private async Task<HttpResponseMessage> CacheContent(Uri url)
    {
        if (cache.Contains(url.ToString()))
        {
            return (HttpResponseMessage)cache.Get(url.ToString());
        }

        HttpResponseMessage response = await AddResilience(url);

        cache.Add(url.ToString(), response, DateTimeOffset.UtcNow.AddMinutes(1));

        return response;
    }

    /// <summary>
    /// Adds resilience to the calling of the web service.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <returns>The response of the WebService</returns>
    /// <exception cref="SimpleHttpClientException"></exception>
    private async Task<HttpResponseMessage> AddResilience(Uri url)
    {
        AsyncCircuitBreakerPolicy circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

        AsyncRetryPolicy retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16)
            }, (exception, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry attempt {retryCount} failed. Waiting {timeSpan} before retrying...");
            });

        AsyncRetryPolicy retryPolicy2 = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                5, // number of retries
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // calculate delay based on retry count
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry attempt {retryCount} failed. Waiting {timeSpan} before retrying...");
                });

        AsyncTimeoutPolicy timeoutPolicy = Policy.TimeoutAsync(30);

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotImplemented);

        try
        {
            await circuitBreakerPolicy.WrapAsync(timeoutPolicy.WrapAsync(retryPolicy)).ExecuteAsync(async () =>
            {
                response = await QueryWebService(url);
            });
        }
        catch (BrokenCircuitException bcex)
        {
            throw new SimpleHttpClientException("There was a persistent error while trying to query the web service.", bcex);
        }

        return response;
    }

    /// <summary>
    /// Queries a web service and returns the response.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <returns>The response of the WebService</returns>
    /// <exception cref="SimpleHttpClientException"></exception>
    private async Task<HttpResponseMessage> QueryWebService(Uri url)
    {
        using var client = new HttpClient();

        try
        {
            return await client.GetAsync(url);
        }
        catch (HttpRequestException hrex)
        {
            throw new SimpleHttpClientException("Error querying the web service.", hrex);
        }
    }
}

/// <summary>
/// Exception thrown by the SimpleHttpClient.
/// </summary>
public class SimpleHttpClientException : Exception
{
    public SimpleHttpClientException()
    {
    }
    public SimpleHttpClientException(string message)
        : base(message)
    {
    }
    public SimpleHttpClientException(string message, Exception inner)
        : base(message, inner)
    {
    }
}






