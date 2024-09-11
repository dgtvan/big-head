using BotApi.Businesses.Constants;
using BotApi.Databases.Models;
using Microsoft.Bot.Schema;
using Thread = BotApi.Databases.Models.Thread;

namespace BotApi.Businesses.Services;

public class InAndOutActivityTrackingService
{
    private readonly BotDbContext _dbContext;

    public InAndOutActivityTrackingService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void TrackIncomingActivity(Activity activity)
    {
        Track(activity);
    }

    public void TrackOutgoingActivity(Activity activity)
    {
        Track(activity);
    }

    private void Track(Activity activity)
    {
        if (activity.Type != ActivityTypes.Message)
        {
            // For simplicity, we only track messages for now.
            return;
        }

        //
        // Author
        //
        var authorReferenceId = activity.From.Id;
        var author = _dbContext.Authors.FirstOrDefault(a => a.ReferenceId == authorReferenceId);
        if (author == null)
        {
            author = new Author
            {
                ReferenceId = authorReferenceId,
                Name = activity.From.Name
            };
            _dbContext.Authors.Add(author);
        }

        //
        // Thread
        //
        Thread? thread = _dbContext.Threads.FirstOrDefault(t => t.ReferenceId == activity.Conversation.Id);
        if (thread == null)
        {
            thread = new Thread()
            {
                ReferenceId = activity.Conversation.Id
            };
            _dbContext.Threads.Add(thread);
        }

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
            throw new NotImplementedException();
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
        _dbContext.Messages.Add(message);


        //
        // Channel
        //
        // TODO: Determine which channel the message comes from.
        // The activity.ChannelId provides a string that identifies the channel, such as:
        //  "msteams" for Microsoft Teams
        //  "slack" for Slack
        //  "emulator" for the Bot Framework Emulator
        //  "directline" for Direct Line

        _dbContext.SaveChanges();
    }
}
