using MediatR;

namespace BotApi.Businesses.Handlers.AzureOpenAi.ShouldRunAssistant;

public class ShouldRunAssistant : IRequestHandler<ShouldRunAssistantRequest, bool>
{
    public async Task<bool> Handle(ShouldRunAssistantRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Use a  light weigh AI model to determine if the message requires an AI response.
        // For now, we always require an AI response.
        return await Task.FromResult(true);
    }
}