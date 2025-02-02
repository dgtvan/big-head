using System.Threading.Channels;

namespace BotApi.Businesses.Services.AIExecutionManagemenetService;

public class AIOperationItem
{
}

public class AIOperationQueueService
{
    private readonly Channel<AIOperationItem> _queue;

    public AIOperationQueueService()
    {
        UnboundedChannelOptions options = new()
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true
        };

        _queue = Channel.CreateUnbounded<AIOperationItem>(options);
    }

    public async Task Enqueue(AIOperationItem item, CancellationToken cancellationToken = default)
    {
        while (await _queue.Writer.WaitToWriteAsync(cancellationToken))
        {
            if (_queue.Writer.TryWrite(item))
            {
                return;
            }
        }
    }

    public async Task<AIOperationItem?> Dequeue(CancellationToken cancellationToken = default)
    {
        while (await _queue.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_queue.Reader.TryRead(out AIOperationItem? operationItem) && operationItem is not null)
            {
                return operationItem;
            }
        }

        return null;
    }
}

