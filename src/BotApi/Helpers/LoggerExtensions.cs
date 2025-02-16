#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Logging;

using System.Text;
using BotApi.Databases.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure
using Thread = BotApi.Databases.Models.Thread;

public class LogContext()
{
    public string? LogRelationId { get; init; }
    public Thread? Thread { get; init; }
    public Message? Message { get; set; }

    public static LogContext Create(Thread? thread, string? runId)
    {
        return new LogContext()
        {
            Thread = thread,
            LogRelationId = runId
        };
    }

    public static LogContext Create(Thread? thread, Message? message)
    {
        return new LogContext()
        {
            Thread = thread,
            Message = message
        };
    }
}

public static class LoggerExtensions
{
#pragma warning disable CA2254 // Template should be a static expression
    public static void BotWarning(this ILogger logger, string message, params object?[] args) => logger.LogWarning(PrependThreadId(null, message), args);
    public static void BotInformation(this ILogger logger, string message, params object?[] args) => logger.LogInformation(PrependThreadId(null, message), args);
    public static void BotError(this ILogger logger,string message, params object?[] args) => logger.LogError(PrependThreadId(null, message), args);

    public static void BotWarning(this ILogger logger, Thread? thread, string message, params object?[] args) => logger.LogWarning(PrependThreadId(thread, message), args);
    public static void BotInformation(this ILogger logger, Thread? thread, string message, params object?[] args) => logger.LogInformation(PrependThreadId(thread, message), args);
    public static void BotError(this ILogger logger, Thread? thread, string message, params object?[] args) => logger.LogError(PrependThreadId(thread, message), args);

    public static void BotWarning(this ILogger logger, LogContext context, string message, params object?[] args) => logger.LogWarning(PrependLogContext(context, message), args);
    public static void BotInformation(this ILogger logger, LogContext context, string message, params object?[] args) => logger.LogInformation(PrependLogContext(context, message), args);
    public static void BotError(this ILogger logger, LogContext context, string message, params object?[] args) => logger.LogError(PrependLogContext(context, message), args);
#pragma warning restore CA2254 // Template should be a static expression

    private static string PrependThreadId(Thread? thread, string message)
    {
        if (thread == null)
        {
            return $"Thread: {NotAvailable}. {message}";
        }

        return $"Thread: Id {thread.Id} (Name: {thread.Name ?? NotAvailable}). {message}";
    }

    private static string PrependLogContext(LogContext context, string logMessage)
    {
        Thread? thread = context.Thread;
        string? logRelationId = context.LogRelationId;
        Message? message = context.Message;
        //Author? author = message?.Author;

        string[] contextData =
            [
                thread is null  ? string.Empty : $"Thread Id {thread.Id} (Name: {thread.Name ?? NotAvailable})",
                logRelationId is null   ? string.Empty : $"Log Relation Id {logRelationId}",
                message is null ? string.Empty : $"Message Id {message.Id}",
                //author is null  ? string.Empty : $"Message Author Id {author.Id} (Name: {author.Name ?? _notAvailable})"
            ];

        string logContext = string.Join(", ", contextData.Where(x => !string.IsNullOrWhiteSpace(x)));

        return $"{logContext}. {logMessage}";
    }

    private static readonly string NotAvailable = "N/A";
}
