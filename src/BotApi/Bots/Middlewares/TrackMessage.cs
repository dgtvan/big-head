using BotApi.Businesses.Services.MessageTrackingService;
using BotApi.Databases.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;

namespace BotApi.Bots.Middlewares;

public class TrackMessage : Microsoft.Bot.Builder.IMiddleware
{
    private readonly ILogger<TrackMessage> _logger;
    private readonly MessageTrackingService _messageTrackingService;

    public TrackMessage(
        ILogger<TrackMessage> logger,
        MessageTrackingService messageTrackingService
    )
    {
        _logger = logger;
        _messageTrackingService = messageTrackingService;
    }

    public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
    {
        IncomingMessage(turnContext);
        OutgoingMessage(turnContext);

        await next(cancellationToken);
    }

    private void IncomingMessage(ITurnContext turnContext)
    {
        if (!_messageTrackingService.ShouldTrack(turnContext.Activity))
        {
            return;
        };

        Message message = _messageTrackingService.TrackIncomingActivity(turnContext.Activity);
        turnContext.TurnState.SetMessage(message);
    }

    private void OutgoingMessage(ITurnContext turnContext)
    {
        SendActivitiesHandler sendActivityHandler = (ITurnContext _, List<Activity> activities, Func<Task<ResourceResponse[]>> next) =>
        {
            activities.ForEach(activity =>
            {
                if (!_messageTrackingService.ShouldTrack(activity))
                {
                    return;
                };

                _messageTrackingService.TrackOutgoingActivity(activity);
            });

            return next();
        };
        turnContext.OnSendActivities(sendActivityHandler);
    }
}
