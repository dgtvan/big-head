using BotApi.Businesses.Services.MessageTrackingService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;

namespace BotApi.Bots.Middlewares;

public class TrackMessage(MessageTrackingService messageTrackingService) : Microsoft.Bot.Builder.IMiddleware
{
    public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
    {
        IncomingMessage(turnContext);
        OutgoingMessage(turnContext);

        await next(cancellationToken);
    }

    private void IncomingMessage(ITurnContext turnContext)
    {
        if (messageTrackingService.ShouldTrack(turnContext.Activity))
        {
            messageTrackingService.TrackIncomingActivity(turnContext.Activity);
        };
    }

    private void OutgoingMessage(ITurnContext turnContext)
    {
        SendActivitiesHandler sendActivityHandler = (ITurnContext _, List<Activity> activities, Func<Task<ResourceResponse[]>> next) =>
        {
            activities.ForEach(activity =>
            {
                if (messageTrackingService.ShouldTrack(activity))
                {
                    messageTrackingService.TrackOutgoingActivity(activity);

                };
            });

            return next();
        };
        turnContext.OnSendActivities(sendActivityHandler);
    }
}
