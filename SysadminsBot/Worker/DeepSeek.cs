using SysadminsBot.Interfaces;

namespace SysadminsBot.Worker;

public class DeepSeek : IAiInterface
{
    public async Task<ForumMessage> Reply(ForumMessage question, Settings settings)
    {
        var deepSeek = new DeepSeekResult(settings.ApiKey);
        question.Answer = await deepSeek.Reply(question.Body);
        return question;
    }
}