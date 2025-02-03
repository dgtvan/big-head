using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Databases.Models;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.State;
using Microsoft.Bot.Builder;

namespace BotApi.Bots;

public class BotApplication : Application<TurnState>
{
    private readonly ILogger<BotApplication> _logger;
    private readonly ThreadService _aiThreadService;

    public BotApplication
    (
        ApplicationOptions<TurnState> options,
        ThreadService aiThreadService
    ) : base(options)
    {
        _logger = Options?.LoggerFactory?.CreateLogger<BotApplication>() ?? throw new NotSupportedException("Logger is not supported");
        _aiThreadService = aiThreadService;
        RegisterHandlers();
    }

    private static Task<bool> AllRoutes(ITurnContext context, CancellationToken cancellation) => Task.FromResult(true);

    protected void RegisterHandlers()
    {
        //Echo(AllRoutes);
        RespondUsingAI(AllRoutes);
    }

    private void Echo(RouteSelectorAsync selector)
    {
        OnActivity(
            selector,
            async (turnContext, state, cancelToken) =>
            {
                //_logger.LogInformation("Received message: {message}", context.Activity.Text);
                await turnContext.SendActivityAsync($"(Echo) {turnContext.Activity.Text}", cancellationToken: cancelToken);
                await Task.CompletedTask;
            }
        );
    }

    private void RespondUsingAI(RouteSelectorAsync selector)
    {
        OnActivity(
            selector,
            async (turnContext, state, cancelToken) =>
            {
                Message? message = turnContext.TurnState.GetMessage();
                if (message is null)
                {
                    return;
                }

                await _aiThreadService.AddMessage(message, cancelToken);

                if (!await _aiThreadService.RequireAIResponse(message, cancelToken))
                {
                    _logger.BotInformation("The user message does not require an AI response. Skip the AI response sending process.");
                    return;
                }

                string aiResponse = await _aiThreadService.RespondToUserMessage(message);

                _logger.BotInformation("Sending the AI response to the user");
                await turnContext.SendActivityAsync(aiResponse, cancellationToken: cancelToken);
                _logger.BotInformation("The AI response has been sent to the user");
            }
        );
    }
}
