using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
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

        //string aiMessageText =
        //        $"""
        //            The message has two parts:
        //                1. Message Author Name: The name of the author of the message.
        //                2. Message Content: The content of the message.

        //            Below is the message detail:
        //                1. Message Author Name: {message.Author?.Name}
        //                2. Message Content: {message.Text}
        //        """;
        string aiMessageText =
                $"""
                    I am {message.Author?.Name}. {message.Text}
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

        message.AiMessageId = aiMessage.Id;
        message.AiMessageText = aiMessageText;
        dbContext.SaveChanges();

        logger.BotInformation(logContext, "Saved the AI Message Text to the database");

        while (aiMessage.Status == MessageStatus.InProgress)
        {
            logger.BotInformation(logContext, "The message creation is running. Waiting for the result...");
            await Task.Delay(1000, cancellationToken);
            aiMessage = await clientProvider.AssistantClient.GetMessageAsync(aiMessage.ThreadId, aiMessage.Id, cancellationToken);
        }

        // I don't know why the Status is always null
        logger.BotInformation(logContext, "Status: {status}", aiMessage.Status);

        logger.BotInformation(logContext, "Created at: {at}", aiMessage.CreatedAt.DateTime);
        if (aiMessage.CompletedAt is not null)
        {
            logger.BotInformation(logContext, "Completed at: {at}", aiMessage.CompletedAt?.DateTime);
        }
        if (aiMessage.IncompleteAt is not null)
        {
            logger.BotInformation(logContext, "InCompleted at: {at}", aiMessage.IncompleteAt?.DateTime);
        }

        if (aiMessage.IncompleteDetails?.Reason is not null)
        {
            logger.BotInformation(logContext, "Incomplete detail: {detail}", aiMessage.IncompleteDetails.Reason);
        }

        logger.BotInformation(logContext, "AI Message Id: {id}", aiMessage.Id);
    }

    public async Task<bool> RequireAIResponse(Message message, CancellationToken cancellationToken = default)
    {
        // TODO: Use a  light weigh AI model to determine if the message requires an AI response.
        // For now, we always require an AI response.
        return await Task.FromResult(true);
    }

    public async Task<string> RespondToUserMessage(Message message, CancellationToken cancellationToken = default)
    {
        await AskAssistantToRespondToUserMessage(message);

        ArgumentNullException.ThrowIfNull(message.Thread?.AiThreadId);
        ArgumentNullException.ThrowIfNull(message.AiMessageId);

        string aiResponseText = await GetTheAssistantMessage(message.Thread.AiThreadId, message.AiMessageId, cancellationToken);
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

    private async Task<string> GetTheAssistantMessage(string aiThreadId, string lastUserMessageId, CancellationToken cancellationToken = default)
    {
        logger.BotInformation("Getting the assistant messages");

        MessageCollectionOptions options = new()
        {
            // Given that the assisant only produces one message after a succesful run, the following means to get the assistant message only.
            // Why is it "BeforeId" instead of "AfterId"? I thought it would be "AferId", however, messages are ordered by descending time stamp by default which means the last message is on top. Therefore, "BeforeId" is the correct one for me.
            BeforeId = lastUserMessageId,
            PageSizeLimit = 1,
        };

        AsyncCollectionResult<ThreadMessage> messages = clientProvider.AssistantClient.GetMessagesAsync(aiThreadId, options, cancellationToken);

        logger.BotInformation("Extracting the assistant message");
        StringBuilder stringBuilder = new();

        ConfiguredCancelableAsyncEnumerable<ThreadMessage>.Enumerator enumerator = messages.WithCancellation(cancellationToken).GetAsyncEnumerator();
        try
        {
            int threadMessageCounter = 1;
            while (await enumerator.MoveNextAsync())
            {
                ThreadMessage assistantMessage = enumerator.Current;
                logger.BotInformation("Reading the thread message {counter}", threadMessageCounter++);

                int messageContentCounter = 1;
                foreach (MessageContent content in assistantMessage.Content)
                {
                    logger.BotInformation("Reading the message content {counter}", messageContentCounter++);

                    // TODO: Handle image, image annotation,...
                    // For now, we only handle the text content.
                    stringBuilder.AppendLine(content.Text);

                    foreach (TextAnnotation annotation in content.TextAnnotations)
                    {
                        logger.BotInformation("Annotation Start index {start}", annotation.StartIndex);
                        logger.BotInformation("Annotation End index {start}", annotation.EndIndex);
                        logger.BotInformation("Annotation TextToReplace {start}", annotation.TextToReplace);
                        logger.BotInformation("Annotation InputField {start}", annotation.InputFileId);
                    }

                    stringBuilder.AppendLine();
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        string finalResponseText = stringBuilder.ToString();

        logger.BotInformation("The assistant message processing has been completed");

        return finalResponseText;
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
                logger.BotInformation(thread, "It is an Emulater thread. We will re-use an existing AI Thread created for Emulator");

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
                logger.BotInformation(thread, "It is an Emulater thread. We will re-use an existing AI Assistant created for Emulator");

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
