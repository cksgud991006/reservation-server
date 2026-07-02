using System.ComponentModel.DataAnnotations;

namespace TicketServer.Api.Dto;

public record EnqueueResponse(
    [Required] bool Success
);

public record TicketWaitResponse(
    [Required] Guid UserId,
    [Required] int Position
);

public record TicketSessionResponse(
    [Required] Guid UserId,
    [Required] long TimeExpiry
);

public record TicketBookResponse(
    [Required] bool Success,
    [Required] string FlightId,
    [Required] DateTimeOffset Date,
    [Required] string SeatNumber,
    [Required] Guid UserId,
    [Required] string BookingId,
    [Required] string Details
);

public record TicketBookFailureResponse(
    [Required] string Details,
    [Required] bool Success
);
