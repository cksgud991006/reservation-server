using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using ReservationServer.Domain.Redis;
using ReservationServer.Application.Schedule;


namespace ReservationServer.Application.BackgroundJobs;
 
public class WaitQueueRunner : BackgroundService, IJobRunner<Guid>
{
    private readonly IDatabase _redis;
    private readonly int _loadCount = 500;
    private readonly TimeSpan _minDelayMs = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _maxDelayS = TimeSpan.FromSeconds(10);
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
            int processed = await RunTask();
            var delay = processed >= _loadCount ? _minDelayMs   // batch full -> set to drain fast
                        : processed == 0 ?  _maxDelayS          // revert delay
                        : TimeSpan.FromSeconds(2);              // default delay

            await Task.Delay(delay, stoppingToken);
        }
    }

    public async Task<int> RunTask()
    {

        // Implementation for running the job worker
        var jobs = await _redis.SortedSetRangeByRankAsync(
                RedisKeys.QueueWaitKey,
                0,
                -1,
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

        return jobs.Length;
    }
}