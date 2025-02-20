using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Databases;
using BotApi.Databases.Enums;
using BotApi.Databases.Models;
using MediatR;
using OpenAI.Assistants;

namespace BotApi.Businesses.Handlers.AzureOpenAi.Setup.Thread;

public class SetupAiThreadHandler(
    ILogger<SetupAiThreadHandler> logger,
    BotDbContext dbContext,
    ClientProviderService clientProvider
) : IRequestHandler<SetupAiThreadRequest>
{
    public async Task Handle(SetupAiThreadRequest request, CancellationToken cancellationToken = default)
    {
        OpenAiThread? openAiThread = dbContext.OpenAiThreads.FirstOrDefault(x => x.ThreadId == request.ThreadId);
        if (openAiThread is not null)
        {
            logger.BotInformation("AI Thread has already been setup");
            return;
        }

        Databases.Models.Thread thread =
            dbContext.Threads.FirstOrDefault(x => x.Id == request.ThreadId)
            ?? throw new NullReferenceException($"Coult not find the Thread with th Id {request.ThreadId}");

        if (thread.Type == ThreadType.Emulator)
        {
            // Look for an existing Open AI Thread created for Emulator
            string? openAiThreadId =
                dbContext.Threads
                    .Join(dbContext.OpenAiThreads,
                        dbThread => dbThread.Id,
                        dbOpenAiThread => dbOpenAiThread.ThreadId,
                        (dbThread, dbOpenAiThread) => new { dbThread, dbOpenAiThread })
                    .Where(joined => joined.dbThread.Type == ThreadType.Emulator)
                    .Select(joined => joined.dbOpenAiThread.OpenAiThreadId)
                    .FirstOrDefault();

            if (openAiThreadId is not null)
            {
                logger.BotInformation(thread, "It is an Emulater thread. We will re-use an existing AI Thread created for Emulator");

                openAiThread = new OpenAiThread
                {
                    ThreadId       = thread.Id,
                    OpenAiThreadId = openAiThreadId
                };
                dbContext.OpenAiThreads.Add(openAiThread);

                dbContext.SaveChanges();

                logger.BotInformation(thread, "Use the AI Thread {aiThreadId}", openAiThreadId);

                return;
            }
        }

        logger.BotInformation(thread, "Creating AI Thread");

        ThreadCreationOptions options = new();

        // According to OpenAI document, vector stores have an expiration time. Details here https://platform.openai.com/docs/assistants/tools/file-search#managing-costs-with-expiration-policies
        //options.ToolResources.FileSearch.VectorStoreIds.Add("playground-store");

        AssistantThread assistantThread = await clientProvider.AssistantClient.CreateThreadAsync(options);

        openAiThread = new OpenAiThread
        {
            ThreadId       = thread.Id,
            OpenAiThreadId = assistantThread.Id
        };

        dbContext.OpenAiThreads.Add(openAiThread);

        dbContext.SaveChanges();

        logger.BotInformation(thread, "Created AI thread");
    }
}
