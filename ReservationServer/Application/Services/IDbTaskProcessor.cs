using ReservationServer.Api.Dto;
using ReservationServer.Infrastructure.Database;

namespace ReservationServer.Application.Services;
public interface IDbTaskProcessor
{
    public Task ProcessTaskAsync(SqlTask sqlTask, FlightDbContext flightDbContext);
}