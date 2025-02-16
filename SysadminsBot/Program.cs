using Microsoft.Extensions.Configuration;

namespace SysadminsBot
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Debug.json",true,true)
                .AddEnvironmentVariables()
                .Build();
            var settings = config.GetRequiredSection("Settings").Get<Settings>();

            // Write the values to the console.
            // Console.WriteLine($"user = {settings?.User}");
            // Console.WriteLine($"password = {settings?.Password}");
            // Console.WriteLine($"topics = {settings?.Topics}");
            if (settings == null)
            {
                Console.WriteLine("settings not exist");
                return;
            }
            var topics = settings.Topics.Select(topic => topic.Url).ToArray();
            var bot = new ChatBot(settings.User, settings.Password,topics, settings.SkipUsers, settings.ApiKey);
            while (true)
            {
                await bot.TalkAsync();
            }
        }
    }
}
