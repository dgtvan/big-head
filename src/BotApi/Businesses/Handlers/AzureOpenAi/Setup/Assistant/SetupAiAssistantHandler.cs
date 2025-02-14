using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Databases;
using BotApi.Databases.Enums;
using BotApi.Databases.Models;
using MediatR;
using OpenAI.Assistants;

namespace BotApi.Businesses.Handlers.AzureOpenAi.Setup.Assistant;

public class SetupAiAssistantHandler(
    ILogger<SetupAiAssistantHandler> logger,
    BotDbContext dbContext,
    ClientProviderService clientProvider
) : IRequestHandler<SetupAiAssistantRequest>
{
    public async Task Handle(SetupAiAssistantRequest request, CancellationToken cancellationToken = default)
    {
        OpenAiAssistant? openAiAssistant = dbContext.OpenAiAssistants.FirstOrDefault(x => x.ThreadId == request.ThreadId);
        if (openAiAssistant is not null)
        {
            logger.BotInformation("AI Assistant has already been setup");
            return;
        }

        Databases.Models.Thread thread =
            dbContext.Threads.FirstOrDefault(x => x.Id == request.ThreadId)
            ?? throw new NullReferenceException($"Coult not find the Thread with th Id {request.ThreadId}");

        if (thread.Type == ThreadType.Emulator)
        {
            string? openAiAssistantId =
                dbContext.Threads
                    .Join(dbContext.OpenAiAssistants,
                        dbThread => dbThread.Id,
                        dbOpenAiAssistant => dbOpenAiAssistant.ThreadId,
                        (dbThread, dbOpenAiAssistant) => new { dbThread, dbOpenAiAssistant })
                    .Where(joined => joined.dbThread.Type == ThreadType.Emulator)
                    .Select(joined => joined.dbOpenAiAssistant.OpenAiAssistantId)
                    .FirstOrDefault();

            if (openAiAssistantId is not null)
            {
                logger.BotInformation("It is an Emulater thread. We will re-use an existing AI Assistant created for Emulator");

                dbContext.OpenAiAssistants.Add(new OpenAiAssistant()
                {
                    ThreadId          = thread.Id,
                    OpenAiAssistantId = openAiAssistantId
                });

                logger.BotInformation("Use AI Sssistant Id {assistantId}", openAiAssistantId);

                return;
            }
        }

        logger.BotInformation("Creating AI Assistant");

        AssistantCreationOptions options = new()
        {
            Description           = "Assistant for the thread {threadId}",
            Instructions          = "You answer my questions in a concise, articulate way. Always prioritise looking for answers from the given reference files. Remember to cite what page and file name you used for the answer.",
            Temperature           = 1.0F,
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

        OpenAI.Assistants.Assistant assistant = await clientProvider.AssistantClient.CreateAssistantAsync("van-gpt-4o-mini-2024-07-18", options);

        dbContext.OpenAiAssistants.Add(new OpenAiAssistant()
        {
            ThreadId          = thread.Id,
            OpenAiAssistantId = assistant.Id
        });

        dbContext.SaveChanges();

        logger.BotInformation("Created AI Assistant");

        logger.BotInformation("AI Assistant Id {assistantId}", assistant.Id);
    }
}
