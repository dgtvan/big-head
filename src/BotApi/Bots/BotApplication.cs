using Microsoft.Teams.AI;
using Microsoft.Teams.AI.State;

namespace BotApi.Bots;

public class BotApplication : Application<TurnState>
{
    private readonly ILogger<BotApplication>? _logger;

    public BotApplication
    (
        ApplicationOptions<TurnState> options
    ) : base(options)
    {
        _logger = Options?.LoggerFactory?.CreateLogger<BotApplication>();
        RegisterHandlers();
    }

    protected void RegisterHandlers()
    {
        OnAllIncomingRequest();
    }

    private void OnAllIncomingRequest()
    {
        OnActivity(
            (context, _) => Task.FromResult(true),
            async (turnContext, state, cancelToken) =>
            {
                //_logger.LogInformation("Received message: {message}", context.Activity.Text);
                await turnContext.SendActivityAsync($"(Echo) {turnContext.Activity.Text}", cancellationToken: cancelToken);
                await Task.CompletedTask;
            }
        );
    }
}
