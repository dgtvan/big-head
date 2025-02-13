using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using BotApi.Businesses.Services.AzureOpenAI;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenAI.Assistants;

namespace BotApi.Businesses.Handlers.AzureOpenAi.GetLatestAssistantMessage;

public class GetLatestAssistantMessageHandler(
    ILogger<GetLatestAssistantMessageHandler> logger,
    ClientProviderService clientProvider
) : IRequestHandler<GetLatestAssistantMessageRequest, GetLatestAssistantMessageResponse>
{
    public async Task<GetLatestAssistantMessageResponse> Handle(GetLatestAssistantMessageRequest request, CancellationToken cancellationToken = default)
    {
        logger.BotInformation("Getting the assistant messages");

        MessageCollectionOptions options = new()
        {
            // Given that the assisant only produces one message after a succesful run, the following means to get the assistant message only.
            // Why is it "BeforeId" instead of "AfterId"? I thought it would be "AferId", however, messages are ordered by descending time stamp by default which means the last message is on top. Therefore, "BeforeId" is the correct one for me.
            //BeforeId      = request.OpenAiLastMessageId,

            // Default is MessageCollectionOrder.Descending
            Order = MessageCollectionOrder.Descending,

            PageSizeLimit = 1,
        };

        AsyncCollectionResult<ThreadMessage> messages = clientProvider.AssistantClient.GetMessagesAsync(request.OpenAiThreadId, options, cancellationToken);

        logger.BotInformation("Extracting the assistant message");
        StringBuilder stringBuilder = new();

        // ReSharper disable once RedundantWithCancellation
        ConfiguredCancelableAsyncEnumerable<ThreadMessage>.Enumerator enumerator = messages.WithCancellation(cancellationToken).GetAsyncEnumerator();
        try
        {
            int threadMessageCounter = 1;
            while (await enumerator.MoveNextAsync())
            {
                ThreadMessage assistantMessage = enumerator.Current;
                logger.BotInformation("Reading the thread message {counter}", threadMessageCounter++);

                if (!assistantMessage.RunId.Equals(request.OpenAiRunId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.BotWarning("Something is not right. There is a message from a different run. The current Run Id is {runId}", assistantMessage.RunId);
                    continue;
                }

                int messageContentCounter = 1;
                foreach (MessageContent content in assistantMessage.Content)
                {
                    logger.BotInformation("Reading the message content {counter}", messageContentCounter++);

                    string contentText = string.Empty;

                    foreach (TextAnnotation annotation in content.TextAnnotations)
                    {
                        logger.BotInformation("Annotation Start index {start}", annotation.StartIndex);
                        logger.BotInformation("Annotation End index {start}", annotation.EndIndex);
                        logger.BotInformation("Annotation TextToReplace {start}", annotation.TextToReplace);
                        logger.BotInformation("Annotation InputField {start}", annotation.InputFileId); // File Id in the vectoer store

                        // TODO: Handle the text annotation
                        // For now, I will just remove the citation data.
                        contentText = content.Text.Replace(annotation.TextToReplace, string.Empty);
                    }

                    // TODO: Handle image, image annotation,...
                    // For now, we only handle the text content.
                    stringBuilder.AppendLine(contentText);

                    stringBuilder.AppendLine();
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        logger.BotInformation("The assistant message processing has been completed");

        GetLatestAssistantMessageResponse response = new()
        {
            Message = stringBuilder.ToString()
        };
        return response;
    }
}
