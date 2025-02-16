using BotApi.Businesses.Handlers.AzureOpenAi.AddUserMessage;
using BotApi.Businesses.Handlers.AzureOpenAi.GetLatestAssistantMessage;
using BotApi.Businesses.Handlers.AzureOpenAi.RunAssistant;
using BotApi.Businesses.Handlers.AzureOpenAi.Setup.Assistant;
using BotApi.Businesses.Handlers.AzureOpenAi.Setup.Thread;
using BotApi.Businesses.Handlers.AzureOpenAi.ShouldRunAssistant;
using BotApi.Businesses.Handlers.MessageTracking.GetTrackedMessage;
using BotApi.Businesses.Services.MessageTrackingService;
using MediatR;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.State;
using Microsoft.Bot.Builder;

namespace BotApi.Bots;

public class BotApplication : Application<TurnState>
{
    private readonly ILogger<BotApplication> _logger;
    private readonly IMediator _mediator;
    private readonly MessageTrackingService _messageTrackingService;

    public BotApplication
    (
        ApplicationOptions<TurnState> options,
        IMediator mediator,
        MessageTrackingService messageTrackingService

    ) : base(options)
    {
        _logger = Options.LoggerFactory?.CreateLogger<BotApplication>() ?? throw new NotSupportedException("Logger is not supported");
        _mediator = mediator;
        _messageTrackingService = messageTrackingService;

        RegisterHandlers();
    }

    private static async Task<bool> IsOnlyEchoRoute(ITurnContext context, CancellationToken cancellation)
    {
        bool isEcho = context.Activity.Text?.StartsWith("echo", StringComparison.OrdinalIgnoreCase) ?? false;
        return await Task.FromResult(isEcho);
    }

    private static async Task<bool> IsNormalRoute(ITurnContext context, CancellationToken cancellation)
    {
        return !await IsOnlyEchoRoute(context, cancellation);
    }

    protected void RegisterHandlers()
    {
        Echo(IsOnlyEchoRoute);
        RespondUsingAi(IsNormalRoute);
    }

    private void Echo(RouteSelectorAsync selector)
    {
        OnActivity(
            selector,
            async (turnContext, _ /* state */, cancelToken) =>
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Text}", cancellationToken: cancelToken);
                await Task.CompletedTask;
            }
        );
    }

    private void RespondUsingAi(RouteSelectorAsync selector)
    {
        OnActivity(
            selector,
            async (turnContext, _ /* state */, cancelToken) =>
            {
                GetTrackedMessageResponse? message = await _mediator.Send(new GetTrackedMessageRequest(turnContext.Activity.Id), cancelToken);
                if (message is null)
                {
                    _logger.BotInformation("The user message has not been tracked. Skip the AI response sending process");
                    return;
                }

                _logger.BotInformation("Forwarding the user message to the AI");

                await _mediator.Send(new SetupAiThreadRequest()
                {
                    ThreadId = message.ThreadId,
                }, cancelToken);

                await _mediator.Send(new SetupAiAssistantRequest()
                {
                    ThreadId = message.ThreadId,
                }, cancelToken);

                await _mediator.Send(new AddUserMessageRequest()
                {
                    MessageId = message.MessageId
                }, cancelToken);

                bool shouldRunAssistant = await _mediator.Send(new ShouldRunAssistantRequest(), cancelToken);
                if (!shouldRunAssistant)
                {
                    _logger.BotInformation("The AI response is not required for the user message");
                    return;
                }

                RunAssistantResponse runAssistantResponse = await _mediator.Send(new RunAssistantRequest()
                {
                    MessageId = message.MessageId
                }, cancelToken);

                GetLatestAssistantMessageResponse assistantResponse = await _mediator.Send(new GetLatestAssistantMessageRequest()
                {
                    OpenAiRunId = runAssistantResponse.OpenAiRunId,
                    OpenAiThreadId = runAssistantResponse.OpenAiThreadId
                });

                if (string.IsNullOrWhiteSpace(assistantResponse.Message))
                {
                    _logger.BotInformation("The AI response is empty. Do not send it to the user");
                    return;
                }

                _logger.BotInformation("Sending the AI response to the user");
                await turnContext.SendActivityAsync(assistantResponse.Message, cancellationToken: cancelToken);
                _logger.BotInformation("AI response has been sent to the user");
            }
        );
    }
}
