using System.ComponentModel.DataAnnotations;

namespace TicketServer.Api.Dto;

public enum TaskType
{
    BookSeat,
    CancelSeat
}
public record SqlTask(
    [Required] TaskType Type,
    [Required] string FlightId,
    [Required] string SeatNumber,
    [Required] Guid UserId
);