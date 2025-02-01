using BotApi.Businesses.Services.AzureOpenAi;
using Microsoft.Bot.Builder;

namespace BotApi.Bots.Middlewares;

public class SetupAi(ThreadService threadService) : Microsoft.Bot.Builder.IMiddleware
{

    public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
    {
        await threadService.SetupThread(turnContext.Activity);
        await next(cancellationToken);
    }
}
