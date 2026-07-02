using System.ComponentModel.DataAnnotations;

namespace TicketServer.Api.Dto;

public enum SqlTaskType
{
    BookSeat,
    CancelSeat
}
public record SqlTask(
    [Required] SqlTaskType Type,
    [Required] string FlightId,
    [Required] string SeatNumber,
    [Required] Guid UserId
);