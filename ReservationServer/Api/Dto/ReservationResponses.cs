using System.ComponentModel.DataAnnotations;

namespace ReservationServer.Api.Dto;

public record EnqueueResponse(
    [Required] bool Success
);

public record ReservationWaitResponse(
    [Required] Guid UserId,
    [Required] int Position
);

public record ReservationSessionResponse(
    [Required] Guid UserId,
    [Required] long TimeExpiry
);

public record ReservationBookResponse(
    [Required] string FlightId,
    [Required] string SeatNumber,
    [Required] Guid UserId,
    [Required] string BookingId,
    [Required] string Details
);

public record ReservationBookFailureResponse(
    [Required] string Details
);