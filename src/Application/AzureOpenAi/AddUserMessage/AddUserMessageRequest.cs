using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.AddUserMessage;

public class AddUserMessageRequest : IRequest
{
    public int MessageId { get; init; }
}