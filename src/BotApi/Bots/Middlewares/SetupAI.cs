using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Databases.Models;
using Microsoft.Bot.Builder;

namespace BotApi.Bots.Middlewares;

public class SetupAI(ThreadService aiThreadService) : Microsoft.Bot.Builder.IMiddleware
{
    public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
    {
        await Handle(turnContext, cancellationToken);
        await next(cancellationToken);
    }

    private async Task Handle(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        Message? message = turnContext.TurnState.GetMessage();

        if (message is null)
        {
            return;
        }

        if (message?.Thread == null)
        {
            throw new Exception($"The message (Id {message?.Id}) does not belong to any thread. This situation should never happened.");
        }

        await aiThreadService.SetupThread(message.Thread, cancellationToken);
    }
}
