namespace BotApi.Databases.Models
{
    public class OpenAiThread
    {
        public int ThreadId { get; set; }
        public string OpenAiThreadId { get; set; } = null!;
    }
}