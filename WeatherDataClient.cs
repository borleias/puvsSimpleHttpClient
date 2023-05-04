using System.Text.Json;

/// <summary>
/// A client for the weather data web service.
/// </summary>
public class WeatherDataClient
{
    /// <summary>
    /// Gets the weather data from the web service.
    /// </summary>
    /// <param name="url">The URL to query. Must not be null.</param>
    /// <returns>The response as WeatherInfo object.</returns>
    /// <exception cref="WeatherDataClientException"></exception>
    public async Task<OperationResult<WeatherInfo>> Get(Uri url)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        return await GetContent(url);
    }

    /// <summary>
    /// Deserializes the JSON response from the web service.
    /// </summary>
    /// <param name="url">The URL to query. Must not be null.</param>
    /// <returns>The response as WeatherInfo object.</returns>
    /// <exception cref="WeatherDataClientException"></exception>
    private async Task<OperationResult<WeatherInfo>> GetContent(Uri url)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        OperationResult<WeatherInfo> result;

        try
        {
            HttpResponseMessage httpResponse = await new SimpleHttpClient().Get(url);

            if (httpResponse.IsSuccessStatusCode)
            {
                string json = await httpResponse.Content.ReadAsStringAsync();
                
                try
                {
                    JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    WeatherInfo response = JsonSerializer.Deserialize<WeatherInfo>(json, options) ?? new WeatherInfo();
                    result = new OperationResult<WeatherInfo>(response);
                }
                catch (JsonException jex)
                {
                    result = new OperationResult<WeatherInfo>("Error deserializing JSON.", jex);
                }
            }
            else
            {
                result = new OperationResult<WeatherInfo>($"Error querying the web service. Status code: {httpResponse.StatusCode}");
            }
        }
        catch (SimpleHttpClientException shex)
        {
            result = new OperationResult<WeatherInfo>("Persistent error querying the web service.", shex);
        }

        return result;
    }
}

/// <summary>
/// Exception thrown by the WeatherDataClient.
/// </summary>
public class WeatherDataClientException : Exception
{
    public WeatherDataClientException()
    {
    }
    public WeatherDataClientException(string message) : base(message)
    {
    }
    public WeatherDataClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// The result of an operation covering positive and negative results.
/// </summary>
/// <typeparam name="T">The type of the expected result.</typeparam>
public class OperationResult<T> where T : class, new()
{
    /// <summary>
    /// The result of the operation in case of a positive result; otherwise null.
    /// </summary>
    public T Result { get; private set; } = new();

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Result is null ? false : true;

    /// <summary>
    /// The error message in case of a negative result; otherwise empty.
    /// </summary>
    public string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    /// The covered exception in case of a negative result; otherwise null
    /// </summary>
    public Exception? Exception { get; private set; } = null;

    /// <summary>
    /// Creates a positive OperationResult based on a result.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    public OperationResult(T result)
    {
        if(result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        this.Result = result;
        this.ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Creates a negative OperationResult based on an error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public OperationResult(string errorMessage) : this(errorMessage, new())
    {
    }

    /// <summary>
    /// Creates a negative OperationResult based on an exception.
    /// </summary>
    /// <param name="exception">The covered exception.</param>
    public OperationResult(Exception exception) : this(exception.Message, exception)
    {
    }

    /// <summary>
    /// Creates a negative OperationResult based on an error message and an exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exception">The exception.</param>
    public OperationResult(string errorMessage, Exception exception)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException($"\"{nameof(errorMessage)}\" darf nicht NULL oder ein Leerraumzeichen sein.", nameof(errorMessage));
        }

        if(exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        this.Exception = exception;
        this.ErrorMessage = errorMessage;
    }
}





