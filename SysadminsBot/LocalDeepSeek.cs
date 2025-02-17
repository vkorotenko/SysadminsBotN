using System.Text.Json;

namespace SysadminsBot;

public class LocalDeepSeek
{
    public async Task<string?> Reply(string body)
    {
        var chat = "http://localhost:11434/api/chat";
        var generate = "http://localhost:11434/api/generate";
        var client = new HttpClient();
        client.Timeout = new TimeSpan(0, 0, 4, 0);
        var request = new HttpRequestMessage(HttpMethod.Post, generate);
        var model = new ReqModel { Prompt = body ,Stream = false};
        var prep = JsonSerializer.Serialize(model);
        var content = new StringContent(prep, null, "application/json");
        request.Content = content;
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        return await response.Content.ReadAsStringAsync();
    }
}