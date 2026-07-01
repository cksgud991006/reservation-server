using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using TicketServer.Domain.Redis;
using TicketServer.Application.Schedule;


namespace TicketServer.Application.BackgroundJobs;
 
public class WaitQueueRunner : BackgroundService, IJobRunner<Guid>
{
    private readonly IDatabase _redis;
    private readonly int _loadCount = 500;
    private int _startCount = 0;
    private readonly ILogger<WaitQueueRunner> _logger;
    public WaitQueueRunner(IConnectionMultiplexer connectionMultiplexer,
                            ILogger<WaitQueueRunner> logger)
    {
        _redis = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Wait queue worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunTask();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public async Task RunTask()
    {

        // Implementation for running the job worker
        var jobs = await _redis.SortedSetRangeByRankAsync(
                RedisKeys.QueueWaitKey,
                _startCount,
                _loadCount,
                Order.Ascending);

        // add to active users and redirect to issuing api
        foreach (var job in jobs)
        {
            var userId = job.ToString();

            var score = await _redis.SortedSetScoreAsync(RedisKeys.QueueWaitKey, userId);
            var timeExpiry = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
            await _redis.SortedSetAddAsync(RedisKeys.QueueActiveKey, userId, timeExpiry);
            await _redis.SortedSetRemoveAsync(RedisKeys.QueueWaitKey, userId);
        }
    }
}