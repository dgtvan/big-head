namespace BotApi.Databases.Models
{
    public class AIMessage
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();
    }
}