using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Assistants;
using OpenAI.Files;
using System.ClientModel;

namespace BotApi.Businesses.Services.AzureOpenAI;

public class ClientProviderService
{
    private readonly ConfigOptions _options;

    private readonly Lazy<AzureOpenAIClient> _azureOpenAIClient;
    private readonly Lazy<OpenAIFileClient> _fileClient;
    private readonly Lazy<AssistantClient> _assistantClient;

    public OpenAIFileClient FileClient => _fileClient.Value;
    public AssistantClient AssistantClient => _assistantClient.Value;

    public ClientProviderService(IOptions<ConfigOptions> options)
    {
        _options = options.Value;

        _azureOpenAIClient = new Lazy<AzureOpenAIClient>(
            new AzureOpenAIClient(
                new Uri(_options.Azure?.OpenAIEndpoint ?? throw new NullReferenceException("Endpoint is not configured")),
                new ApiKeyCredential(_options.Azure?.OpenAIApiKey ?? throw new NullReferenceException("API Key is not configured"))
            )
        );

        _fileClient = new Lazy<OpenAIFileClient>(
            _azureOpenAIClient.Value.GetOpenAIFileClient()
        );


        _assistantClient = new Lazy<AssistantClient>(
            _azureOpenAIClient.Value.GetAssistantClient()
        );
    }
}
