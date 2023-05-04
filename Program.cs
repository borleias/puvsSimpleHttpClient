// puvsHttpClient
//
// Dies ist ein Client für einen WebService Aufruf.

partial class Program
{
    static async Task Main(string[] args)
    {
        const string url = "https://api.open-meteo.com/v1/forecast?latitude=52.28&longitude=13.62&hourly=temperature_2m&current_weather=true";

        bool exit = false;

        Console.WriteLine("### puvsSimpleHttpClient ###");

        do
        {
            OperationResult<WeatherInfo> operationResult = await new WeatherDataClient().Get(new Uri(url));

            if(operationResult.IsSuccess) 
            {
                WeatherInfo data = operationResult.Result;

                Console.WriteLine();
                Console.WriteLine($"{DateTime.Now} \n{data}");
            }
            else
            {
                Console.WriteLine($"Error downloading weather data: {operationResult.ErrorMessage}");
            }

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





