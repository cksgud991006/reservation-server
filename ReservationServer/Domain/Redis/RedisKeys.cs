using System;
using ReservationServer.Domain.Database;

namespace ReservationServer.Domain.Redis;

public static class RedisKeys
{
    // Queue keys for the high-concurrency ticket scheduling pipeline
    public const string QueueWaitKey = "queue:waiting:zset";
    public const string QueueActiveKey = "queue:active:zset";
    public const string SqlTaskKey = "sql:tasks:list";
    private const string FlightInstanceNamespace = "flight_instance";
    private const string FlightSeatCountNamespace = "flight_seat_count";
    private const string SeatLayoutNamespace = "seat_layout";
    private const string FlightBookingNamespace = "flight_booking";
    private const string BookingNamespace = "booking";

    /// <summary>
    /// Generates: flight_instance:{flightNumber}:{departureTime}
    /// Example: flight_instance:AA100:20260617_211500
    /// </summary>
    public static string FlightInstance(string flightNumber, string departureTime)
    {
        // Use a compact, URL/Key-safe date format (yyyyMMdd_HHmmss) 
        // and enforce uppercase for consistency
    
        return $"{FlightInstanceNamespace}:{flightNumber.ToUpperInvariant()}:{departureTime}";
    }

    /// <summary>
    /// Generates: flight_seat_count:{flightId}
    /// </summary>
    public static string FlightSeatCount(string flightId)
    {
        return $"{FlightSeatCountNamespace}:{flightId}";
    }

    /// <summary>
    /// Generates: seat_layout:{flightNumber}:{seatNumber}
    /// </summary>
    public static string SeatLayout(string flightNumber, string seatNumber) 
        => $"{SeatLayoutNamespace}:{flightNumber.ToUpperInvariant()}:{seatNumber.ToUpperInvariant()}";

    /// <summary>
    /// Generates: flight_booking:{flightId}:{seatNumber}
    /// </summary>
    public static string FlightBooking(string flightId, string seatNumber)
    {
        return $"{FlightBookingNamespace}:{flightId}:{seatNumber.ToUpperInvariant()}";
    }

    /// <summary>
    /// Generates: booking:{bookingId}
    /// </summary>
    public static string Booking(string bookingId) => $"{BookingNamespace}:{bookingId}";

    /// <summary>
    /// Standardized hash field or set member identifier for a unique seat.
    /// e.g., "ECONOMY:C167"
    /// </summary>
    public static string GetSeatField(ClassType classType, string seatNumber)
        => $"{classType.ToString().ToUpperInvariant()}:{Sanitize(seatNumber)}";

    private static string Sanitize(string input) 
        => input?.Trim().ToUpperInvariant() ?? throw new ArgumentNullException(nameof(input));

}