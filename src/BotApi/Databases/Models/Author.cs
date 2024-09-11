namespace BotApi.Databases.Models;

public class Author
{
    public int Id { get; set; }
    public required string ReferenceId { get; set; }
    public required string Name { get; set; }
}
