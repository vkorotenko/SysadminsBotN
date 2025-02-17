namespace SysadminsBot.Interfaces;

public interface IAiInterface
{
    Task<ForumMessage> Reply(ForumMessage question, Settings settings);
}