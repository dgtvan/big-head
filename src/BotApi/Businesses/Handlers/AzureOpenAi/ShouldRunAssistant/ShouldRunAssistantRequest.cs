using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.ShouldRunAssistant;

public class ShouldRunAssistantRequest : IRequest<bool>
{
    public int MessageId { get; set; }
}