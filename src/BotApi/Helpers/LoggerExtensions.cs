#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Logging;
#pragma warning restore IDE0130 // Namespace does not match folder structure
using Thread = BotApi.Databases.Models.Thread;

public static class LoggerExtensions
{
#pragma warning disable CA2254 // Template should be a static expression
    public static void BotWarning(this ILogger logger, Thread? thread, string message, params object?[] args) => logger.LogWarning(PrependThreadId(thread, message), args);
    public static void BotInformation(this ILogger logger, Thread? thread, string message, params object?[] args) => logger.LogInformation(PrependThreadId(thread, message), args);
    public static void BotError(this ILogger logger, Thread? thread, string message, params object?[] args) => logger.LogError(PrependThreadId(thread, message), args);
#pragma warning restore CA2254 // Template should be a static expression

    private static string PrependThreadId(Thread? thread, string message)
    {
        if (thread == null)
        {
            return $"Thread: N/A. {message}";
        }

        return $"Thread: Id {thread.Id} (Name: {thread.Name ?? "N/A"}). {message}";
    }
}
