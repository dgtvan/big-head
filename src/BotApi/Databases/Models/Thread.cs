namespace BotApi.Databases.Models;

public class Thread
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ReferenceId { get; set; }
    public ThreadType Type { get; set; }
}

public enum ThreadType
{
    Meeting,
    Group,
    Personal
}
