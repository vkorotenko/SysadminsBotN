using System.Text.Json.Serialization;

namespace SysadminsBot
{
    public class ForumMessage
    {
        public string Author { get; set; }
        public string Mid { get; set; }
        public string Pid { get; set; }
        public string Body { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = "";
    }
}
