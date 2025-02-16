namespace BotApi.Businesses.Services.AIExecutionManagemenetService;

public class AIExecutionService(
    ILogger<AIExecutionService> logger,
    AIOperationQueueService operationQueue
) : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.BotInformation("AI Execution Service started.");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.BotInformation("AI Execution Service stopped.");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            AIOperationItem? operationItem = await operationQueue.Dequeue(stoppingToken);
            if (operationItem != null)
            {
                logger.BotWarning("Dequeue was success but the result was null. It should not happen quite often.");
                continue;
            }

            // TODO: Process the operation item
        }
    }
}
