using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using TicketServer.Domain.Redis;

namespace TicketServer.Schedule;
public class JobRunner : IJobRunner
{
    private readonly IDatabase _redis;
    private readonly int _loadCount = 500;
    private int _startCount = 0;

    public JobRunner(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }
    
    public async Task RunAsync()
    {

        // Implementation for running the job worker
        var jobs = await _redis.SortedSetRangeByRankAsync(
                RedisKeys.JobWaitingKey,
                _startCount,
                _loadCount,
                Order.Ascending);

        // add to active users and redirect to issuing api
        foreach (var job in jobs)
        {
            var userId = job.ToString();

            var score = await _redis.SortedSetScoreAsync(RedisKeys.JobWaitingKey, userId);
            var timeExpiry = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
            await _redis.SortedSetAddAsync(RedisKeys.JobActiveKey, userId, timeExpiry);
            await _redis.SortedSetRemoveAsync(RedisKeys.JobWaitingKey, userId);
        }
    }
}
