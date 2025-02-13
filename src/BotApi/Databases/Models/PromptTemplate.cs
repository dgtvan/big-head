namespace BotApi.Databases.Models
{
    public class PrompTemplate
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Prompt { get; set; } = null!;
    }
}