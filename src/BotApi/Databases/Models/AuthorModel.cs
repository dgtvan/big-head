namespace BotApi.Databases.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();
    }
}