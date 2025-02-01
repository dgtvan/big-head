using Microsoft.Bot.Schema;
using OpenAI.Assistants;
using Assistant = OpenAI.Assistants.Assistant;
using Thread = BotApi.Databases.Models.Thread;

namespace BotApi.Businesses.Services.AzureOpenAi;

public class ThreadService(ILogger<ThreadService> logger, BotDbContext dbContext, ClientProviderService clientProvider)
{
    public async Task<string?> SetupThread(Activity activity, CancellationToken cancellationToken = default)
    {
        Thread? thread = dbContext.Threads.FirstOrDefault(t => t.ReferenceId == activity.Conversation.Id);
        if (thread == null)
        {
            logger.BotWarning(null, "The reference thread id {threadId} has not been tracked. Could not setup AI Thread for the given thread.", activity.Conversation.Id);
            return null;
        }

        await SetupThread(thread);
        await SetupAssistant(thread);

        return thread.AiThreadId;
    }

    private async Task SetupThread(Thread thread)
    {
        if (thread.AiThreadId is not null)
        {
            logger.BotInformation(thread, "AI Thread has already been setup");
            return;
        }

        logger.BotInformation(thread, "Creating AI thread");

        ThreadCreationOptions options = new ThreadCreationOptions();

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

        logger.BotInformation(thread, "Creating AI Assistant");

        AssistantCreationOptions options = new AssistantCreationOptions()
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
