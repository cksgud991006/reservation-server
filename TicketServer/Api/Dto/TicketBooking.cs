using System.ComponentModel.DataAnnotations;

namespace TicketServer.Api.Dto;

public record PostResponse(
    [Required] bool Success
);

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

public record TicketWaitResponse(
    [Required] Guid UserId,
    [Required] int Position
);

public record TicketSessionResponse(
    [Required] Guid UserId,
    [Required] long TimeExpiry
);

public record TicketIssueResponse(
    [Required] bool Success,
    [Required] string FlightId,
    [Required] DateTimeOffset Date,
    [Required] string SeatNumber,
    [Required] Guid UserId,
    [Required] string BookingId,
    [Required] string Details
);

public record TicketIssueFailure(
    [Required] string Details,
    [Required] bool Success
);
