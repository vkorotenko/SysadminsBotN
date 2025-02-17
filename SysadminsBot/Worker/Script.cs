using SysadminsBot.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace SysadminsBot.Worker
{
    public class Script : IAiInterface
    {
        public async Task<ForumMessage> Reply(ForumMessage question, Settings settings)
        {
            var temp = Path.Combine(System.IO.Path.GetTempPath(),"msg.json");
            var json = JsonSerializer.Serialize(question);
            await File.WriteAllTextAsync(temp,json);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.ScriptPath,
                    Arguments = temp
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            json = await File.ReadAllTextAsync(temp);
            var r = JsonSerializer.Deserialize<ForumMessage>(json);
            return r;
        }
    }
}
