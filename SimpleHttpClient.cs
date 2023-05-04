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
        return await CacheContent(url,
                                  30); // seconds
    }

    /// <summary>
    /// Caches the response of the WebService for a certain number of seconds.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <param name="retentionTimeInSeconds">The number of seconds to cache the response.</param>
    /// <returns>The response of the WebService</returns>
    /// <exception cref="SimpleHttpClientException"></exception>
    private async Task<HttpResponseMessage> CacheContent(Uri url, int retentionTimeInSeconds)
    {
        if (cache.Contains(url.ToString()))
        {
            return (HttpResponseMessage)cache.Get(url.ToString());
        }

        HttpResponseMessage response = await AddResilience(url,
                                                           5,   // retries
                                                           30); // seconds

        cache.Add(url.ToString(), response, DateTimeOffset.UtcNow.AddSeconds(retentionTimeInSeconds));

        return response;
    }

    /// <summary>
    /// Adds resilience to the calling of the web service.
    /// After a certain number of retries, the circuit breaker will open 
    /// and the web service will not be called anymore in the next 60 seconds.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <param name="numberOfRetries">The number of retries in case of failure, e.g. timeout.</param>
    /// <param name="timeoutInSeconds">The timeout in seconds after which a web service call will be terminated.</param>
    /// <returns>The response of the WebService</returns>
    /// <exception cref="SimpleHttpClientException"></exception>
    private async Task<HttpResponseMessage> AddResilience(Uri url, int numberOfRetries, int timeoutInSeconds)
    {
        AsyncCircuitBreakerPolicy circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(2 * numberOfRetries, TimeSpan.FromSeconds(60));

        AsyncRetryPolicy retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                numberOfRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // calculate delay based on retry count
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry attempt {retryCount} failed. Waiting {timeSpan} before retrying...");
                });

        AsyncTimeoutPolicy timeoutPolicy = Policy.TimeoutAsync(timeoutInSeconds);

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotImplemented);

        try
        {
            await circuitBreakerPolicy.WrapAsync(retryPolicy.WrapAsync(timeoutPolicy)).ExecuteAsync(async () =>
            {
                response = await QueryWebService(url);
            });
        }
        catch (HttpRequestException hrex)
        {
            throw new SimpleHttpClientException("Error querying the web service.", hrex);
        }
        catch (TimeoutRejectedException trex)
        {
            throw new SimpleHttpClientException("The web service call timed out.", trex);
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






