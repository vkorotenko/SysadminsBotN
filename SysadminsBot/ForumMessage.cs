using System.Text.Json.Serialization;

namespace SysadminsBot
{
    public class ForumMessage
    {
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("mid")]
        public string Mid { get; set; }
        [JsonPropertyName("pid")]
        public string Pid { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = "";
    }
}
