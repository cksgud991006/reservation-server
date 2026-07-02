using System.ComponentModel.DataAnnotations;

namespace TicketServer.Api.Dto;

public record TicketBookRequest(
    [Required] string FlightId,
    [Required] string SeatNumber,
    [Required] Guid UserId
);

public record TicketWaitRequest(
    [Required] Guid UserId,
    [Required] DateTimeOffset RequestTime,
    [Required] string IdempotencyKey
);
