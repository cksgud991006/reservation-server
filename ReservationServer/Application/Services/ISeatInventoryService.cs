using ReservationServer.Domain.Database;
using ReservationServer.Domain.Response;

namespace ReservationServer.Application.Services;

public interface ISeatInventoryService
{
    public Task<SeatInventoryResponse> ReserveSeatAsync(
        string flightId, 
        string seatNumber, 
        Guid id);
}