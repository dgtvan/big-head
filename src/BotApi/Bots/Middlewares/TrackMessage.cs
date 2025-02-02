using BotApi.Businesses.Services.MessageTrackingService;
using BotApi.Databases.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

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
        // Handle incoming messages
        IncomingMessage(turnContext);

        // Handle outgoing messages.
        SendActivitiesHandler sendActivityHandler = (ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next) =>
        {
            activities.ForEach(activity =>
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    OutgoingMessage(turnContext);
                }
            });

            return next();
        };
        turnContext.OnSendActivities(sendActivityHandler);

        await next(cancellationToken);
    }

    private void IncomingMessage(ITurnContext turnContext)
    {
        if (!_messageTrackingService.ShouldTrack(turnContext.Activity))
        {
            return;
        };

        Message message = _messageTrackingService.TrackIncomingActivity(turnContext.Activity);
        SetMessageContext(turnContext, message);
    }

    private void OutgoingMessage(ITurnContext turnContext)
    {
        if (!_messageTrackingService.ShouldTrack(turnContext.Activity))
        {
            return;
        };

        Message message = _messageTrackingService.TrackOutgoingActivity(turnContext.Activity);
        SetMessageContext(turnContext, message);
    }

    private static void SetMessageContext(ITurnContext turn, Message message)
    {
        turn.TurnState.SetMessage(message);
    }
}
