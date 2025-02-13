using MediatR;

namespace BotApi.Businesses.Handlers.MessageTracking.GetTrackedMessage;

public record GetTrackedMessageRequest(string ReferenceId) : IRequest<GetTrackedMessageResponse?>
{
}