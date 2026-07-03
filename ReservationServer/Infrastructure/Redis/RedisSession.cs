using ReservationServer.Infrastructure.Redis;
using ReservationServer.Domain.Redis;
using ReservationServer.Domain.Response;
using StackExchange.Redis;

namespace ReservationServer.Infrastructure.Redis;

public class RedisSession: IRedisSession
{
    private readonly IDatabase _redis;
    public RedisSession(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();   
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(
        Guid id)
    {
        var key = RedisKeys.QueueActiveKey;
        var score = await _redis.SortedSetScoreAsync(key, id.ToString());

        if (!score.HasValue)
        {
            return SessionStatusResponse.NotActive();
        }

        var timeExpiry = DateTimeOffset.FromUnixTimeSeconds((long)score.Value);

        return SessionStatusResponse.Active(timeExpiry);
    }
}