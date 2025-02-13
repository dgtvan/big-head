using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.RunAssistant;

public class RunAssistantRequest : IRequest<RunAssistantResponse>
{
    public required int MessageId { get; init; }
}