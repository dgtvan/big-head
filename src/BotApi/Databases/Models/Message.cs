namespace BotApi.Databases.Models;

public class Message
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public string? AiText { get; set; }
    public required DateTime Timestamp { get; set; }
    public int AuthorId { get; set; }
    public Author? Author { get; set; }
    public int ThreadId { get; set; }
    public Thread? Thread { get; set; }
}
