using System.Text;

/// <summary>
/// The current weather data.
/// </summary>
public class CurrentWeatherData
{
    /// <summary>
    /// The time of the weather data.
    /// </summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// The temperature in degrees Celsius.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// The current wind speed in km/h.
    /// </summary>
    public double WindSpeed { get; set; }

    /// <summary>
    /// The current wind direction in degrees.
    /// </summary>
    public double WindDirection { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Current Weather:");
        sb.AppendLine($"Time: {Time}");
        sb.AppendLine($"Temperature: {Temperature}°C");
        sb.Append($"Wind Speed: {WindSpeed} km/h at {WindDirection} Degrees");
        return sb.ToString();
    }
}






