namespace TicketServer.Domain.Response;

public class SeatInventoryResponse
{
    public bool Success { get; private set; }
    public string? FlightId { get; private set; }

    public DateTimeOffset Date { get; private set; }

    public string? SeatNumber { get; private set; }
    public Guid UserId { get; private set; } = Guid.Empty;

    public string BookingId { get; private set; } = string.Empty;
    public string? Details { get; private set; }

    private SeatInventoryResponse(bool success, string flightId,  string seatNumber, Guid userId, string bookingId, string details)
    {
        Success = success;
        FlightId = flightId;
        SeatNumber = seatNumber;
        UserId = userId;
        BookingId = bookingId;
        Details = details;

        Date = DateTimeOffset.UtcNow;
    }

    public static SeatInventoryResponse CreateSuccessResponse(string flightId, string seatNumber, Guid userId, string bookingId, string details) =>
        new SeatInventoryResponse(true, flightId, seatNumber, userId, bookingId, details);

    public static SeatInventoryResponse AlreadyReservedResponse(string flightId, string seatNumber, Guid userId, string bookingId, string details) =>
        new SeatInventoryResponse(false, flightId, seatNumber, userId, bookingId, details);

    public static SeatInventoryResponse CreateFailureResponse(string flightId, string seatNumber, Guid userId, string details) =>
        new SeatInventoryResponse(false, flightId, seatNumber, userId, string.Empty, details);

    public static SeatInventoryResponse NoAvailableSeatsResponse(string flightId, string seatNumber, Guid userId, string details) =>
        new SeatInventoryResponse(false, flightId, seatNumber, userId, string.Empty, details);
}