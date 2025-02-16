using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.Setup.Assistant;

public class SetupAiAssistantRequest : IRequest
{
    public required int ThreadId { get; init; }
}