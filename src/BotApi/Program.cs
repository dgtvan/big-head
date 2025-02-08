using System.Reflection;
using BotApi;
using BotApi.Bots;
using BotApi.Bots.Adapters;
using BotApi.Bots.Middlewares;
using BotApi.Businesses.Services.AzureOpenAI;
using BotApi.Businesses.Services.MessageTrackingService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.Models;
using Microsoft.Teams.AI.AI.Planners;
using Microsoft.Teams.AI.AI.Prompts;
using Microsoft.Teams.AI.State;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Emulator"))
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Create the Bot Framework Authentication to be used with the Bot Adapter.
var config = builder.Configuration.Get<ConfigOptions>();
builder.Services.AddOptions<ConfigOptions>().Bind(builder.Configuration);

builder.Configuration["MicrosoftAppType"] = "MultiTenant";
builder.Configuration["MicrosoftAppId"] = config?.BOT_ID;
builder.Configuration["MicrosoftAppPassword"] = config?.BOT_PASSWORD;
builder.Services.AddScoped<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Cloud Adapter with error handling enabled.
// Note: some classes expect a BotAdapter and some expect a BotFrameworkHttpAdapter, so
// register the same adapter instance for both types.
builder.Services.AddScoped<CloudAdapter, AdapterWithErrorHandler>();
builder.Services.AddScoped<IBotFrameworkHttpAdapter>(sp => sp.GetRequiredService<CloudAdapter>());
builder.Services.AddScoped<BotAdapter>(sp => sp.GetRequiredService<CloudAdapter>());
builder.Services.AddScoped<IStorage, MemoryStorage>();
builder.Services.AddScoped<TrackMessage>();
builder.Services.AddScoped<SetupAI>();
builder.Services.AddScoped<BotApplicationBuilder>();
builder.Services.AddScoped<IBot, BotApplication>(sp =>
    sp.GetRequiredService<BotApplicationBuilder>().BuildBot()
);

builder.Services.AddDbContext<BotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BotDatabase"))
, ServiceLifetime.Transient); // TODO: A multi-threaded like Web Application. DBContext should be transient?

builder.Services.AddScoped<MessageTrackingService>();

builder.Services.AddSingleton<ClientProviderService>();
builder.Services.AddScoped<ThreadService>();


//if (!string.IsNullOrWhiteSpace(config.OpenAI?.ApiKey))
//{
//    builder.Services.AddSingleton<OpenAIModel>(sp => new(
//        new OpenAIModelOptions(
//            config.OpenAI.ApiKey, 
//            "gpt-3.5-turbo"
//        )
//        {
//            LogRequests = true
//        },
//        sp.GetService<ILoggerFactory>()
//    ));
//}
//else if (!string.IsNullOrWhiteSpace(config.Azure?.OpenAIApiKey) && !string.IsNullOrWhiteSpace(config.Azure.OpenAIEndpoint))
//{
//    builder.Services.AddSingleton<OpenAIModel>(sp => new(
//        new AzureOpenAIModelOptions(
//            config.Azure.OpenAIApiKey,
//            "gpt-35-turbo",
//            config.Azure.OpenAIEndpoint
//        )
//        {
//            LogRequests = true
//        },
//        sp.GetService<ILoggerFactory>()
//    ));
//}
//else
//{
//    throw new Exception("Missing configuration, please configure settings for either OpenAI or Azure");
//}

// Create the bot as transient. In this case the ASP Controller is expecting an IBot.
//builder.Services.AddScoped<IBot>(sp =>
//{
//    //Create loggers
//    ILoggerFactory loggerFactory = sp.GetService<ILoggerFactory>();

// Create Prompt Manager
//PromptManager prompts = new(new()
//{
//    PromptFolder = "./Bots/Prompts"
//});

// Create ActionPlanner
//ActionPlanner<TurnState> planner = new(
//    options: new(
//        model: sp.GetService<OpenAIModel>(),
//        prompts: prompts,
//        defaultPrompt: async (context, state, planner) =>
//        {
//            PromptTemplate template = prompts.GetPrompt("Chat");
//            return await Task.FromResult(template);
//        }
//    )
//    { LogRepairs = true },
//    loggerFactory: loggerFactory
//);

//    Application<TurnState> app = new ApplicationBuilder<TurnState>()
//        //.WithAIOptions(new(planner))
//        .WithStorage(sp.GetService<IStorage>())
//        .WithLoggerFactory(loggerFactory)
//        .Build();

//    return app;
//});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Emulator"))
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
