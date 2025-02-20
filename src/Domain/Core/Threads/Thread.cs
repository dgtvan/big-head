using BotApi.Databases.Enums;

namespace BotApi.Databases.Models
{
    public class Thread
    {
        public int Id { get; set; }
        public required string ReferenceId { get; set; }
        public string? Name { get; set; }
        public ThreadType Type { get; set; }
    }
}