using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using ReservationServer.Domain.Redis;
using ReservationServer.Api.Dto;
using System.Text.Json;

namespace ReservationServer.Application.Schedule;

public class SqlTaskScheduler : IJobScheduler<SqlTask>
{
    private readonly IDatabase _redis;
    public SqlTaskScheduler(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<int> GetWaitingPositionAsync(SqlTask task)
    {
        string serializedTask = JsonSerializer.Serialize(task);
        var rank = await _redis.SortedSetRankAsync(RedisKeys.SqlTaskKey, serializedTask);
        if (rank.HasValue)
        {
            return (int)rank.Value + 1; // Convert zero-based rank to one-based position
        }
        
        return -1; // Not found
    }

    public async Task ScheduleAsync(SqlTask task, DateTimeOffset scheduleTime)
    {
        string serializedTask = JsonSerializer.Serialize(task);

        await _redis.SortedSetAddAsync(RedisKeys.SqlTaskKey, serializedTask, scheduleTime.ToUnixTimeSeconds());
    }
}