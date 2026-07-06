using ReservationServer.Application.Repositories;
using ReservationServer.Domain.Response;
using ReservationServer.Application.Schedule;

namespace ReservationServer.Application.Services;

public class QueueService: IQueueService
{
    private readonly ILogger<IQueueService> _logger;
    private readonly IJobScheduler<Guid> _jobScheduler;
    public QueueService(ILogger<IQueueService> logger,
                        IJobScheduler<Guid> jobScheduler)
    {
        _logger = logger;
        _jobScheduler = jobScheduler;   
    }

    public async Task<QueueResponse> GetPositionInQueueAsync(
        Guid id)
    {
        var position = await _jobScheduler.GetWaitingPositionAsync(id);

        if (position == -1)
        {
            return QueueResponse.NotInQueue();
        }

        return QueueResponse.InQueue(position);
    }

    public async Task EnqueueAsync(
        Guid id,
        DateTimeOffset RequestTime)
    {

        _logger.LogInformation("Received queue request. UserId: {UserId}", id);

        await _jobScheduler.ScheduleAsync(id, RequestTime);
        
    }
}