using SysadminsBot.Interfaces;

namespace SysadminsBot.Worker;

public class LocalDeep : IAiInterface
{
    public async Task<ForumMessage> Reply(ForumMessage question, Settings settings)
    {
        var lc = new LocalDeepSeek();
        question.Answer = await lc.Reply(question.Body);
        return question;
    }
}