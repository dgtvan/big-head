namespace BotApi.Databases.Models
{
    public class OpenAiMessage
    {
        public required int MessageId { get; set; }
        public required string OpenAiMessageId { get; set; }
        public required string Text { get; set; }
    }
}