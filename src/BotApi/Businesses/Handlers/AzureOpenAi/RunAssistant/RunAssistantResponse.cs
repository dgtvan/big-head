namespace BotApi.Businesses.Handlers.AzureOpenAi.RunAssistant;

public class RunAssistantResponse
{
    public required string OpenAiThreadId { get; init; }
    public required string OpenAiRunId { get; init; }
}