using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Databases;
using BotApi.Databases.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAI.Assistants;

namespace BotApi.Businesses.Handlers.AzureOpenAi.AddUserMessage;

public class AddUserMessageHandler(
    ILogger<AddUserMessageHandler> logger,
    BotDbContext dbContext,
    ClientProviderService clientProvider
) : IRequestHandler<AddUserMessageRequest>
{
    public async Task Handle(AddUserMessageRequest request, CancellationToken cancellationToken = default)
    {
        Message message =
            dbContext.Messages.AsNoTracking().FirstOrDefault(x => x.Id == request.MessageId)
            ?? throw new NullReferenceException($"Could not find the message with the Message Id {request.MessageId}");

        Author author =
            dbContext.Authors.AsNoTracking().FirstOrDefault(x => x.Id == message.AuthorId)
            ?? throw new NullReferenceException($"Could not find the author with the Author Id {message.AuthorId}");

        OpenAiThread openAiThread =
            dbContext.OpenAiThreads.AsNoTracking().FirstOrDefault(x => x.ThreadId == message.ThreadId)
            ?? throw new NullReferenceException($"Could not find the OpenAI Thread with the Thread Id {message.ThreadId}");

        logger.BotInformation("Creating a new message to the AI Thread");

        // TODO: Suggestion for more friendly reference response.
        //        Include references after the answer in this format: """
        //# References
        //**[Section Title Here](https://section-link-here.com/path)**
        //> Exact content body of the references
        //"""
        // TODO: Query prompt of the database
        string prompt =
                $"""
                    I am {author.Name}. {message.Text}
                """;

        string openAiMessageId = await PushUserMessageToOpenAiThread(prompt, openAiThread.OpenAiThreadId);

        OpenAiMessage openAiMessage = new()
        {
            MessageId = request.MessageId,
            Text = prompt,
            OpenAiMessageId = openAiMessageId
        };
        dbContext.OpenAiMessages.Add(openAiMessage);

        dbContext.SaveChanges();

        logger.BotInformation("Saved the AI Message Text to the database");
    }

    private async Task<string> PushUserMessageToOpenAiThread(string message, string openAiThreadId, CancellationToken cancellationToken = default)
    {
        logger.BotInformation("API calling has been completed");

        MessageCreationOptions options = new();

        ThreadMessage openAiMessage = await clientProvider.AssistantClient.CreateMessageAsync(
            openAiThreadId,
            MessageRole.User,
            [ message ],
            options,
            cancellationToken
        );

        while (openAiMessage.Status == MessageStatus.InProgress)
        {
            logger.BotInformation("The message creation is running. Waiting for the result...");
            await Task.Delay(1000, cancellationToken);
            openAiMessage = await clientProvider.AssistantClient.GetMessageAsync(openAiMessage.ThreadId, openAiMessage.Id, cancellationToken);
        }

        // I don't know why the Status is always null
        logger.BotInformation("Status: {status}", openAiMessage.Status);

        logger.BotInformation("Created at: {at}", openAiMessage.CreatedAt.DateTime);
        if (openAiMessage.CompletedAt is not null)
        {
            logger.BotInformation("Completed at: {at}", openAiMessage.CompletedAt?.DateTime);
        }
        if (openAiMessage.IncompleteAt is not null)
        {
            logger.BotInformation("InCompleted at: {at}", openAiMessage.IncompleteAt?.DateTime);
        }

        if (openAiMessage.IncompleteDetails?.Reason is not null)
        {
            logger.BotInformation("Incomplete detail: {detail}", openAiMessage.IncompleteDetails.Reason);
        }

        logger.BotInformation("AI Message Id: {id}", openAiMessage.Id);

        logger.BotInformation("AI Thread Id: {id}", openAiThreadId);

        return openAiMessage.Id;
    }
}
