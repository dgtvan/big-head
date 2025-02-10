namespace BotApi.Databases.Models
{
    public class AIAssistant
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<AIThread> AIThreads { get; set; } = new HashSet<AIThread>();
    }
}