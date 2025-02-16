using BotApi.Databases.Enums;

namespace BotApi.Databases.Models
{
    public class Thread
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public ThreadType Type { get; set; }
    }
}