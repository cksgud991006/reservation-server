using TicketServer.Domain.Response;

namespace TicketServer.Application.Services;

public interface IQueueService
{
    public Task<QueueResponse> GetPositionInQueueAsync(
        Guid id);

    public Task EnqueueAsync(
        Guid id,
        DateTimeOffset RequestTime);
}