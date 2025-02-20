using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.Setup.Thread;

public class SetupAiThreadRequest : IRequest
{
    public required int ThreadId { get; init; }
}