using BotApi.Databases.Models;
using Microsoft.EntityFrameworkCore;
using OpenAI.Assistants;
using Assistant = OpenAI.Assistants.Assistant;
using Thread = BotApi.Databases.Models.Thread;

namespace BotApi.Businesses.Services.AzureOpenAI;

public class ThreadService(ILogger<ThreadService> logger, BotDbContext dbContext, ClientProviderService clientProvider)
{
    public async Task<Thread> SetupThread(Thread thread, CancellationToken cancellationToken = default)
    {
        await SetupThread(thread);
        await SetupAssistant(thread);
        return thread;
    }

    public async Task AddMessage(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message.Thread);

        var logContext = LogContext.Create(message.Thread, message);

        logger.BotInformation(logContext, "Creating a new message to the AI Thread");

        string aiMessageText =
                $"""
                    The message has two parts:
                        1. Message Author Name: The name of the author of the message.
                        2. Message Content: The content of the message.

                    Below is the message detail:
                        1. Message Author Name: {message.Author?.Name}
                        2. Message Content: {message.Text}
                """;

        MessageCreationOptions options = new()
        {
        };

        ThreadMessage aiMessage = await clientProvider.AssistantClient.CreateMessageAsync(
            message.Thread.AiThreadId,
            MessageRole.User,
            [ aiMessageText ],
            options,
            cancellationToken
        );

        logger.BotInformation(logContext, "API calling has been completed");

        message.AiText = aiMessageText;
        dbContext.SaveChanges();

        logger.BotInformation(logContext, "Saved the AI Message Text to the database");

        while (aiMessage.Status == MessageStatus.InProgress)
        {
            logger.BotInformation(logContext, "The message creation is running. Waiting for the result...");
            await Task.Delay(1000, cancellationToken);
            aiMessage = await clientProvider.AssistantClient.GetMessageAsync(aiMessage.ThreadId, aiMessage.Id, cancellationToken);
        }

        logger.BotInformation(logContext, "Status: {status}", aiMessage.Status);

        if (aiMessage.IncompleteDetails?.Reason is not null)
        {
            logger.BotInformation(logContext, "Incomplete detail: {detail}", aiMessage.IncompleteDetails.Reason);
        }
    }

    public async Task<bool> RequireAIResponse(Message message, CancellationToken cancellationToken = default)
    {
        // TODO: Use a  light weigh AI model to determine if the message requires an AI response.
        // For now, we always require an AI response.
        return await Task.FromResult(true);
    }

    public async Task<string> RespondToUserMessage(Message message)
    {
        await AskAssistantToRespondToUserMessage(message);
        
        string aiResponseText = await GetTheAssistantMessage();
        return aiResponseText;
    }

    private async Task AskAssistantToRespondToUserMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message.Thread);

        var thread = message.Thread;
        var logRelationId = Guid.NewGuid().ToString();
        var logContext = LogContext.Create(thread, logRelationId);

        logger.BotInformation(logContext, "Creating a run over a message from the user {authorId} (Name: {authorName}", message.AuthorId, message.Author?.Name ?? "N/A");

        RunCreationOptions options = new()
        {
            ResponseFormat = AssistantResponseFormat.Text,

            // It throws exception if I explicitly set it. Probably, because I'm using the pre-release version.
            //TruncationStrategy= RunTruncationStrategy.Auto, 
        };

        //options.AdditionalMessages.Add(new ThreadInitializationMessage(
        //    MessageRole.User,
        //    [
        //        message.Text
        //    ]
        //));

        ThreadRun runResult = await clientProvider.AssistantClient.CreateRunAsync(thread.AiThreadId, thread.AiAssistantId, options);

        logger.BotInformation(logContext, "The run has been created");


        // Reference for the RunStatus: https://platform.openai.com/docs/assistants/deep-dive
        RunStatus[] runningStatus = [RunStatus.RequiresAction, RunStatus.Queued, RunStatus.InProgress, RunStatus.Cancelling];
        while (runningStatus.Contains(runResult.Status))
        {
            logger.BotInformation(logContext, "The run is still running. Waiting for the result...");
            await Task.Delay(1000);
            runResult = await clientProvider.AssistantClient.GetRunAsync(thread.AiThreadId, runResult.Id);
        }

        logger.BotInformation(logContext, "The run has been completed");

        logger.BotInformation(logContext, "Status: {status}", runResult.Status);

        runResult.RequiredActions.ToList().ForEach(action =>
        {
            logger.BotInformation(logContext, "Required action: {action}", action);
        });

        if (runResult.IncompleteDetails?.Reason is not null)
        {
            logger.BotInformation(logContext, "Incomplete detail: {detail}", runResult.IncompleteDetails.Reason);
        }

        if (runResult.LastError is not null)
        {
            logger.BotInformation(logContext, "Last Error (Code: {code}: {error}", runResult.LastError.Code, runResult.LastError.Message);
        }

    }

    private async Task<string> GetTheAssistantMessage()
    {
        return await Task.FromResult(string.Empty);
    }

    private async Task SetupThread(Thread thread)
    {
        if (thread.AiThreadId is not null)
        {
            logger.BotInformation(thread, "AI Thread has already been setup");
            return;
        }

        if (thread.Type == ThreadType.Emulator)
        {
            var tmpThread = dbContext.Threads.AsNoTracking().FirstOrDefault(t => t.Type == ThreadType.Emulator && t.AiThreadId != null);
            if (tmpThread != null)
            {
                logger.BotInformation(thread, "It is an Emulater thread. We will re-use the shared AI Thread");

                thread.AiThreadId = tmpThread.AiThreadId;
                dbContext.SaveChanges();

                logger.BotInformation(thread, "Use the AI Thread {aiThreadId}", thread.AiThreadId);
                return;
            }
        }

        logger.BotInformation(thread, "Creating AI thread");

        var options = new ThreadCreationOptions();

        // According to OpenAI document, vector stores have an expiration time. Details here https://platform.openai.com/docs/assistants/tools/file-search#managing-costs-with-expiration-policies
        //options.ToolResources.FileSearch.VectorStoreIds.Add("playground-store");

        AssistantThread assistantThread = await clientProvider.AssistantClient.CreateThreadAsync(options);
        thread.AiThreadId = assistantThread.Id;
        dbContext.SaveChanges();

        logger.BotInformation(thread, "Created AI thread");
    }

    private async Task SetupAssistant(Thread thread)
    {
        if (thread.AiAssistantId is not null)
        {
            logger.BotInformation(thread, "AI Assistant has already been setup");
            return;
        }

        if (thread.Type == ThreadType.Emulator)
        {
            var tmpThread = dbContext.Threads.AsNoTracking().FirstOrDefault(t => t.Type == ThreadType.Emulator && t.AiAssistantId != null);
            if (tmpThread != null)
            {
                logger.BotInformation(thread, "It is an Emulater thread. We will re-use the shared AI Assistant.");

                thread.AiAssistantId = tmpThread.AiAssistantId;
                dbContext.SaveChanges();

                logger.BotInformation(thread, "Use the AI Assistant {aiAssistantId}", thread.AiAssistantId);
                return;
            }
        }

        logger.BotInformation(thread, "Creating AI Assistant");

        var options = new AssistantCreationOptions()
        {
            Description = "Assistant for the thread {threadId}",
            Instructions = "You answer my questions in a concise, articulate way. Always prioritise looking for answers from the given reference files. Remember to cite what page and file name you used for the answer.",
            Temperature = 1.0F,
            NucleusSamplingFactor = 1.0F,
        };

        // Enable the file search tool.
        options.Tools.Add(ToolDefinition.CreateFileSearch());

        // Each thread should have its own assistant with its own vector store (knowledge base).
        // For now, to make it simple, all assistant use the same vector store "playground-store".
        options.ToolResources = new ToolResources()
        {
            FileSearch = new FileSearchToolResources()
        };
        options.ToolResources.FileSearch.VectorStoreIds.Add("vs_ssMco3uOKftXr0vZdHAulqEs"); // Vector store name: playground-store

        Assistant assistant = await clientProvider.AssistantClient.CreateAssistantAsync("van-gpt-4o-mini-2024-07-18", options);
        thread.AiAssistantId = assistant.Id;
        dbContext.SaveChanges();

        logger.BotInformation(thread, "Created AI Assistant");
    }
}
