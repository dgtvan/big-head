using BotApi.Businesses.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace BotApi.Bots.Middlewares;

public class InAndOutActivityTracking : Microsoft.Bot.Builder.IMiddleware
{
    private readonly ILogger<InAndOutActivityTracking> _logger;
    private readonly InAndOutActivityTrackingService _inAndOutActivityTrackingService;

    public InAndOutActivityTracking(
        ILogger<InAndOutActivityTracking> logger,
        InAndOutActivityTrackingService inAndOutActivityTrackingService
    )
    {
        _logger = logger;
        _inAndOutActivityTrackingService = inAndOutActivityTrackingService;
    }

    public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
    {
        // Handle incoming messages
        IncomingMessage(turnContext.Activity);

        // Handle outgoing messages.
        SendActivitiesHandler sendActivityHandler = (ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next) =>
        {
            activities.ForEach(activity =>
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    OutgoingMessage(activity);
                }
            });

            return next();
        };
        turnContext.OnSendActivities(sendActivityHandler);

        await next(cancellationToken);
    }

    private void IncomingMessage(Activity activity)
    {
        _inAndOutActivityTrackingService.TrackIncomingActivity(activity);
    }

    private void OutgoingMessage(Activity activity)
    {
        _inAndOutActivityTrackingService.TrackOutgoingActivity(activity);
    }
}
