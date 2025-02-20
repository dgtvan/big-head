using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.GetLatestAssistantMessage;

public class GetLatestAssistantMessageRequest : IRequest<GetLatestAssistantMessageResponse>
{
    public required string OpenAiThreadId { get; init; }
    public required string OpenAiRunId { get; init; }

}