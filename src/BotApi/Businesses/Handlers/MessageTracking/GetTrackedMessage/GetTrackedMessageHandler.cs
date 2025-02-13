using BotApi.Databases;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BotApi.Businesses.Handlers.MessageTracking.GetTrackedMessage;

public class GetTrackedMessageHandler(BotDbContext dbContext)
: IRequestHandler<GetTrackedMessageRequest, GetTrackedMessageResponse?>
{
    public async Task<GetTrackedMessageResponse?> Handle(GetTrackedMessageRequest request, CancellationToken cancellationToken)
    {
        GetTrackedMessageResponse? response =
            await dbContext.Messages
                .AsNoTracking()
                .Where(m => m.ReferenceId == request.ReferenceId)
                .Select(x => new GetTrackedMessageResponse()
                {
                    ThreadId = x.ThreadId,
                    MessageId = x.Id
                })
                .FirstOrDefaultAsync();

        return response;
    }
}