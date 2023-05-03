using System.Text.Json;

/// <summary>
/// A client for the weather data web service.
/// </summary>
public class WeatherDataClient
{
    /// <summary>
    /// Gets the weather data from the web service.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <returns>The response as WeatherInfo object.</returns>
    /// <exception cref="WeatherDataClientException"></exception>
    public async Task<WeatherInfo> Get(Uri url)
    {
        return await GetContent(url);
    }

    /// <summary>
    /// Deserializes the JSON response from the web service.
    /// </summary>
    /// <param name="url">The URL to query.</param>
    /// <returns>The response as WeatherInfo object.</returns>
    /// <exception cref="WeatherDataClientException"></exception>
    private async Task<WeatherInfo> GetContent(Uri url)
    {
        WeatherInfo response = new WeatherInfo();

        var httpResponse = await new SimpleHttpClient().Get(url);

        if (httpResponse.IsSuccessStatusCode)
        {
            string json = await httpResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                response = JsonSerializer.Deserialize<WeatherInfo>(json, options) ?? new WeatherInfo();
            }
            catch (JsonException jex)
            {
                throw new WeatherDataClientException("Error deserializing JSON.", jex);
            }
        }

        return response;
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






