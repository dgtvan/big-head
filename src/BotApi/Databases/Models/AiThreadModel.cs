namespace BotApi.Databases.Models
{
    public class AIThread
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;

        // Foreign Key
        public int? AiAssistantId { get; set; }

        // Navigation Properties
        public AIAssistant? AiAssistant { get; set; }
        public ICollection<Thread> Threads { get; set; } = new HashSet<Thread>();
    }
}