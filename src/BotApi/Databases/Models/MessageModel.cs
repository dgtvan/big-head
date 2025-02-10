namespace BotApi.Databases.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public int AuthorId { get; set; }
        public int ThreadId { get; set; }

        // Foreign Key
        public int? AiMessageId { get; set; }

        // Navigation Properties
        public Author Author { get; set; } = null!;
        public Thread Thread { get; set; } = null!;
        public AIMessage? AiMessage { get; set; }
    }
}