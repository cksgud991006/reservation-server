using ReservationServer.Domain.Response;

namespace ReservationServer.Infrastructure.Redis;

public interface IRedisSession
{
    public Task<SessionStatusResponse> GetSessionStatusAsync(
        Guid id);
}