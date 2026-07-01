using TicketServer.Domain.Database;
using TicketServer.Domain.Response;

namespace TicketServer.Application.Services;

public interface ISeatInventoryService
{
    public Task<SeatInventoryResponse> ReserveSeatAsync(
        string flightId, 
        string seatNumber, 
        Guid id);
}