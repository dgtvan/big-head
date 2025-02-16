using BotApi.Databases.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Bot.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class TurnStateCollectionExtensions
{
    private const string MessageKey = "Message";

    public static void SetMessage(this TurnContextStateCollection turnContext, Message message) => Set(turnContext, MessageKey, message);
    public static Message? GetMessage(this TurnContextStateCollection turnContext) => Get<Message>(turnContext, MessageKey);

    private static void Set<T>(this TurnContextStateCollection turnContext, string key, T value) where T : class
    {
        if (!turnContext.ContainsKey(MessageKey))
        {
            turnContext.Add<T>(key, value);
        }
        else
        {
            turnContext.Set(key, value);
        }
    }

    private static T? Get<T>(this TurnContextStateCollection turnContext, string key) where T : class
    {
        if (!turnContext.ContainsKey(key))
        {
            return null;
        }
        else
        {
            return turnContext.Get<T>(key);
        }
    }
}
