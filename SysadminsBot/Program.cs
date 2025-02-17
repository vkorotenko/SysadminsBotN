using Microsoft.Extensions.Configuration;

namespace SysadminsBot
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Debug.json",true,true)
                .AddEnvironmentVariables()
                .Build();
            var settings = config.GetRequiredSection("Settings").Get<Settings>();

            if (settings == null)
            {
                Console.WriteLine("settings not exist");
                return;
            }
            var bot = new ChatBot(settings);
            while (true)
            {
                await bot.TalkAsync();
            }
        }
    }
}
