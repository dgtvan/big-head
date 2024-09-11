using Microsoft.Teams.AI.State;
using Microsoft.Teams.AI;
using Microsoft.Bot.Builder;

namespace BotApi.Bots;

public class BotApplicationBuilder : ApplicationBuilder<TurnState>
{
    private IServiceProvider _serviceProvider;
    private readonly ApplicationBuilder<TurnState> _builder;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IStorage _storage;

    public BotApplicationBuilder
    (
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        IStorage storage
    )
    {
        _serviceProvider = serviceProvider;
        _builder = new ApplicationBuilder<TurnState>();
        _loggerFactory = loggerFactory;
        _storage = storage;
    }

    public BotApplication BuildBot()
    {
        Application<TurnState> app =
             WithLoggerFactory(_loggerFactory)
            .WithStorage(_storage)
            .Build();

        return ActivatorUtilities.CreateInstance<BotApplication>(_serviceProvider, app.Options);
    }
}
