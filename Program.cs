// puvsHttpClient
//
// Dies ist ein Client für einen WebService Aufruf.

partial class Program
{
    static async Task Main(string[] args)
    {
        const string url = "https://api.open-meteo.com/v1/forecast?latitude=52.28&longitude=13.62&hourly=temperature_2m&current_weather=true";

        bool exit = false;

        WeatherInfo data = null;

        Console.WriteLine("### puvsSimpleHttpClient ###");

        do
        {
            try
            {
                data = await new WeatherDataClient().Get(new Uri(url));
            }
            catch(WeatherDataClientException wdcex)
            {
                Console.WriteLine($"Error downloading weather data: {wdcex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now} \n{data}");
            Console.WriteLine();

            Console.WriteLine("\nPress any key to repreat or X to exit...");
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.X)
            {
                exit = true;
            }
        }
        while (!exit);

        Console.WriteLine("\nGoodbye!");
    }
}





