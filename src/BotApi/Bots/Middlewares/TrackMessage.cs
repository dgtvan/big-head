using System.Diagnostics;
using BotApi.Businesses.Services.MessageTrackingService;
using BotApi.Databases.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Teams.AI.State;
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
        // Handle incoming messages
        Message? incommingMessage = IncomingMessage(turnContext.Activity);
        if (incommingMessage != null)
        {
            turnContext.TurnState.SetMessage(incommingMessage);
        }

        // Handle outgoing messages.
        SendActivitiesHandler sendActivityHandler = (ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next) =>
        {
            activities.ForEach(activity =>
            {
                OutgoingMessage(activity);
            });

            return next();
        };
        turnContext.OnSendActivities(sendActivityHandler);

        await next(cancellationToken);
    }

    private Message? IncomingMessage(Activity activity)
    {
        if (!_messageTrackingService.ShouldTrack(activity))
        {
            return null;
        };

        Message message = _messageTrackingService.TrackIncomingActivity(activity);
        return message;
    }

    private void OutgoingMessage(Activity activity)
    {
        if (!_messageTrackingService.ShouldTrack(activity))
        {
            return;
        };

        _messageTrackingService.TrackOutgoingActivity(activity);
    }
}
