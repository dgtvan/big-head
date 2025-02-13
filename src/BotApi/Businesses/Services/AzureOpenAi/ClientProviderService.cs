using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Assistants;
using OpenAI.Files;
using System.ClientModel;

namespace BotApi.Businesses.Services.AzureOpenAI;

public class ClientProviderService
{
    private readonly Lazy<OpenAIFileClient> _fileClient;
    private readonly Lazy<AssistantClient> _assistantClient;

    public OpenAIFileClient FileClient => _fileClient.Value;
    public AssistantClient AssistantClient => _assistantClient.Value;

    public ClientProviderService(IOptions<ConfigOptions> options)
    {
        var azureOpenAiClient = new Lazy<AzureOpenAIClient>(
            new AzureOpenAIClient(
                new Uri(options.Value.Azure?.OpenAIEndpoint ?? throw new NullReferenceException("Endpoint is not configured")),
                new ApiKeyCredential(options.Value.Azure?.OpenAIApiKey ?? throw new NullReferenceException("API Key is not configured"))
            )
        );

        _fileClient = new Lazy<OpenAIFileClient>(
            azureOpenAiClient.Value.GetOpenAIFileClient()
        );

        _assistantClient = new Lazy<AssistantClient>(
            azureOpenAiClient.Value.GetAssistantClient()
        );
    }
}
