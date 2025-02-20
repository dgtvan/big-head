namespace BotApi.Databases.Models
{
    public class Message
    {
        public int Id { get; set; }
        public required string ReferenceId { get; set; }
        public required string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public int AuthorId { get; set; }
        public int ThreadId { get; set; }
    }
}