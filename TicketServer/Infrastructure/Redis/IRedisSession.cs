using TicketServer.Domain.Response;

namespace TicketServer.Infrastructure.Redis;

public interface IRedisSession
{
    public Task<SessionStatusResponse> GetSessionStatusAsync(
        Guid id);
}