using BotApi.Businesses.Constants;
using BotApi.Databases.Models;
using Microsoft.Bot.Schema;
using Thread = BotApi.Databases.Models.Thread;

namespace BotApi.Businesses.Services.MessageTrackingService;

public class MessageTrackingService(ILogger<MessageTrackingService> logger, BotDbContext dbContext)
{
    public Message TrackIncomingActivity(Activity activity)
    {
        return Track(activity);
    }

    public Message TrackOutgoingActivity(Activity activity)
    {
        Message message = Track(activity);
        return message;
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

    private Message Track(Activity activity)
    {
        //
        // Author
        //
        var authorReferenceId = activity.From.Id;
        var author = dbContext.Authors.FirstOrDefault(a => a.ReferenceId == authorReferenceId);
        if (author == null)
        {
            author = new Author
            {
                ReferenceId = authorReferenceId,
                Name = activity.From.Name
            };
            dbContext.Authors.Add(author);
        }

        //
        // Thread
        //
        var thread = dbContext.Threads.FirstOrDefault(t => t.ReferenceId == activity.Conversation.Id);
        if (thread == null)
        {
            thread = new Thread()
            {
                ReferenceId = activity.Conversation.Id
            };
            dbContext.Threads.Add(thread);
        }

        if (activity.Conversation.ConversationType is null)
        {
            thread.Type = ThreadType.Emulator;
        }
        else
        {
            if (activity.Conversation.ConversationType.Equals(TeamsConversationType.GroupChat, StringComparison.OrdinalIgnoreCase))
            {
                thread.Type = ThreadType.Meeting;
            }
            else if (activity.Conversation.ConversationType.Equals(TeamsConversationType.Personal, StringComparison.OrdinalIgnoreCase))
            {
                thread.Type = ThreadType.Personal;
            }
            else
            {
                throw new NotImplementedException($"Not implemented ConversationType {activity.Conversation.ConversationType}");
            }
        }

        if (thread is null)
        {
            throw new NullReferenceException("Thread is null");
        }


        //
        // Message
        //
        var message = new Message
        {
            Author = author,
            Thread = thread,
            Text = activity.Text,
            Timestamp = activity.Timestamp?.UtcDateTime ?? DateTime.UtcNow
        };
        dbContext.Messages.Add(message);


        //
        // Channel
        //
        // TODO: Determine which channel the message comes from.
        // The activity.ChannelId provides a string that identifies the channel, such as:
        //  "msteams" for Microsoft Teams
        //  "slack" for Slack
        //  "emulator" for the Bot Framework Emulator
        //  "directline" for Direct Line

        dbContext.SaveChanges();

        return message;
    }
}
