using System.Text;

/// <summary>
/// The weather data.
/// </summary>
public class WeatherInfo
{
    /// <summary>
    /// The latitude of the location.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// The longitude of the location.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// The elevation of the location.
    /// </summary>
    public double Elevation { get; set; }

    /// <summary>
    /// The time it took to generate the response in milliseconds.
    /// </summary>
    public double GenerationTime_Ms { get; set; }

    /// <summary>
    /// The current weather data.
    /// </summary>
    public CurrentWeatherData Current_Weather { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Latitude: {Latitude}, Longitude: {Longitude}, Elevation: {Elevation}");
        sb.AppendLine($"Generation Time (ms): {GenerationTime_Ms}");
        
        sb.AppendLine();
        sb.AppendLine($"{Current_Weather}");

        return sb.ToString();
    }
}






