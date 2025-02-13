using BotApi.Businesses._Shared.Constants;
using BotApi.Businesses.Constants;
using BotApi.Databases;
using BotApi.Databases.Enums;
using BotApi.Databases.Models;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Thread = BotApi.Databases.Models.Thread;

namespace BotApi.Businesses.Services.MessageTrackingService;

public class MessageTrackingService(ILogger<MessageTrackingService> logger, BotDbContext dbContext)
{
    public void TrackIncomingActivity(Activity activity)
    {
        Track(activity);
    }

    public void TrackOutgoingActivity(Activity activity)
    {
        Track(activity);
    }

    public bool ShouldTrack(Activity activity)
    {
        if (activity.Type != ActivityTypes.Message)
        {
            // For simplicity, we only track messages for now.
            logger.BotInformation(
                "Activity type is {activityType}, which is not a message." +
                " We ignore it completely, no any actions applied to it e.g. Tracking, AI...",
                activity.Type);

            return false;
        }

        return true;
    }

    public int? GetMessageId(Activity activity)
    {
        return dbContext.Messages.AsNoTracking().FirstOrDefault(m => m.ReferenceId == activity.Id)?.Id;
    }

    private void Track(Activity activity)
    {
        Author author = TrackAuthor(activity);
        Thread thread = TrackThread(activity);
        TrackMessage(activity, author, thread);
        TrackChannel(activity);
    }

    private Author TrackAuthor(Activity activity)
    {
        string? authorReferenceId = activity.From.Id;

        Author? author = dbContext.Authors.FirstOrDefault(a => a.ReferenceId == authorReferenceId);
        if (author == null)
        {
            author = new Author
            {
                ReferenceId = authorReferenceId,
                Name        = activity.From.Name
            };

            dbContext.Authors.Add(author);

            dbContext.SaveChanges();
        }

        return author;
    }

    private Thread TrackThread(Activity activity)
    {
        Thread? thread = dbContext.Threads.FirstOrDefault(t => t.ReferenceId == activity.Conversation.Id);

        if (thread == null)
        {
            thread = new Thread()
            {
                ReferenceId = activity.Conversation.Id
            };
            dbContext.Threads.Add(thread);
        }

        if (IsEmulator(activity))
        {
            thread.Type = ThreadType.Emulator;
        }
        else if (activity.Conversation.ConversationType.Equals(TeamsConversationType.GroupChat,
                StringComparison.OrdinalIgnoreCase))
        {
            thread.Type = ThreadType.Meeting;
        }
        else if (activity.Conversation.ConversationType.Equals(TeamsConversationType.Personal,
                     StringComparison.OrdinalIgnoreCase))
        {
            thread.Type = ThreadType.Personal;
        }
        else
        {
            throw new NotImplementedException(
                $"Not implemented ConversationType {activity.Conversation.ConversationType}");
        }

        dbContext.SaveChanges();

        return thread;
    }

    private void TrackMessage(Activity activity, Author author, Thread thread)
    {
        Message message = new()
        {
            ReferenceId = activity.Id ?? BotIdentity.MessageReferenceId,
            AuthorId    = author.Id,
            ThreadId    = thread.Id,
            Text        = activity.Text,
            Timestamp   = activity.Timestamp?.UtcDateTime ?? DateTime.UtcNow
        };

        dbContext.Messages.Add(message);

        dbContext.SaveChanges();
    }

    private void TrackChannel(Activity _)
    {
        // TODO: Determine which channel the message comes from.
        // The activity.ChannelId provides a string that identifies the channel, such as:
        //  "msteams" for Microsoft Teams
        //  "slack" for Slack
        //  "emulator" for the Bot Framework Emulator
        //  "directline" for Direct Line
    }

    private static bool IsEmulator(Activity activity)
    {
        return activity.Conversation.ConversationType is null;
    }
}