using System.ComponentModel.DataAnnotations;

namespace ReservationServer.Api.Dto;

public record ReservationBookRequest(
    [Required] string FlightId,
    [Required] string SeatNumber,
    [Required] Guid UserId
);

public record ReservationWaitRequest(
    [Required] Guid UserId,
    [Required] DateTimeOffset RequestTime,
    [Required] string IdempotencyKey
);
