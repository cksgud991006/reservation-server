using TicketServer.Api.Dto;
using TicketServer.Infrastructure.Database;

namespace TicketServer.Application.Services;
public interface ISqlTaskProcessor
{
    public Task ProcessSqlTaskAsync(SqlTask sqlTask, FlightDbContext flightDbContext);
}