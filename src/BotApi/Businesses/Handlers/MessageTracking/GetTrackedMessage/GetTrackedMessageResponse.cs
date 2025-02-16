namespace BotApi.Businesses.Handlers.MessageTracking.GetTrackedMessage;

public class GetTrackedMessageResponse
{
    public required int ThreadId { get; init; }
    public required int MessageId { get; init; }
}