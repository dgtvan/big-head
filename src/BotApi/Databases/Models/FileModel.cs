namespace BotApi.Databases.Models
{
    public class File
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileHashSha512 { get; set; } = string.Empty;
    }
}