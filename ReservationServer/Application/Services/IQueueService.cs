using ReservationServer.Domain.Response;

namespace ReservationServer.Application.Services;

public interface IQueueService
{
    public Task<QueueResponse> GetPositionInQueueAsync(
        Guid id);

    public Task EnqueueAsync(
        Guid id,
        DateTimeOffset RequestTime);
}