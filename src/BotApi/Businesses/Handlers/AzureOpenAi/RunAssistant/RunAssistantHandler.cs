using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Databases;
using BotApi.Databases.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAI.Assistants;

namespace BotApi.Businesses.Handlers.AzureOpenAi.RunAssistant;

public class RunAssistantHandler(
    ILogger<RunAssistantHandler> logger,
    BotDbContext dbContext,
    ClientProviderService clientProvider
) : IRequestHandler<RunAssistantRequest, RunAssistantResponse>
{
    public async Task<RunAssistantResponse> Handle(RunAssistantRequest request, CancellationToken cancellationToken)
    {
        OpenAiMessage? openAiMessage = dbContext.OpenAiMessages.AsNoTracking().FirstOrDefault(x => x.MessageId == request.MessageId);
        if (openAiMessage is null)
        {
            throw new NullReferenceException($"The Message with the Message Id {request.MessageId} has not been added to the OpenAI Thread");
        }

        Message message =
            dbContext.Messages.AsNoTracking().FirstOrDefault(x => x.Id == request.MessageId)
            ?? throw new NullReferenceException($"Could not find the message with the Message Id {request.MessageId}");

        OpenAiThread openAiThread =
            dbContext.OpenAiThreads.AsNoTracking().FirstOrDefault(x => x.ThreadId == message.ThreadId)
            ?? throw new NullReferenceException($"Could not find the OpenAI Thread with the Thread Id {message.ThreadId}");

        OpenAiAssistant openAiAssistant =
            dbContext.OpenAiAssistants.AsNoTracking().FirstOrDefault(x => x.ThreadId == openAiThread.ThreadId)
            ?? throw new NullReferenceException($"Could not find the OpenAI Assistant with the Thread Id {openAiThread.ThreadId}");

       string runId = await RunAssistant(openAiThread.OpenAiThreadId, openAiAssistant.OpenAiAssistantId);

       return new RunAssistantResponse()
       {
           OpenAiThreadId = openAiThread.OpenAiThreadId,
           OpenAiRunId = runId
       };
    }

    private async Task<string> RunAssistant(string openAiThreadId, string openAiAssistantId)
    {
        logger.BotInformation("Creating a run over a message from the user");

        RunCreationOptions options = new()
        {
            ResponseFormat = AssistantResponseFormat.Text,

            // It throws exception if I explicitly set it. Probably, because I'm using the pre-release version.
            //TruncationStrategy= RunTruncationStrategy.Auto,
        };
        ThreadRun runResult = await clientProvider.AssistantClient.CreateRunAsync(openAiThreadId, openAiAssistantId, options);

        logger.BotInformation("The run has been created");

        // Reference for the RunStatus: https://platform.openai.com/docs/assistants/deep-dive
        RunStatus[] runningStatus = [RunStatus.RequiresAction, RunStatus.Queued, RunStatus.InProgress, RunStatus.Cancelling];
        while (runningStatus.Contains(runResult.Status))
        {
            logger.BotInformation("The run is still running. Waiting for the result...");
            await Task.Delay(1000);
            runResult = await clientProvider.AssistantClient.GetRunAsync(openAiThreadId, runResult.Id);
        }

        logger.BotInformation("The run has been completed");

        logger.BotInformation("Status: {status}", runResult.Status);

        runResult.RequiredActions.ToList().ForEach(action =>
        {
            logger.BotInformation("Required action: {action}", action);
        });

        if (runResult.IncompleteDetails?.Reason is not null)
        {
            logger.BotInformation("Incomplete detail: {detail}", runResult.IncompleteDetails.Reason);
        }

        if (runResult.LastError is not null)
        {
            logger.BotInformation("Last Error (Code: {code}: {error}", runResult.LastError.Code,
                runResult.LastError.Message);
        }

        logger.BotInformation("AI Thread ID: {threadId}", runResult.ThreadId);
        logger.BotInformation("AI Assistant ID: {assistantId}", runResult.AssistantId);
        logger.BotInformation("AI Run ID: {runId}", runResult.Id);

        return runResult.Id;
    }
}