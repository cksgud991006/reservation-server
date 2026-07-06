using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ReservationServer.Core;

namespace ReservationServer.Domain.Database;    

[PrimaryKey(nameof(FlightId), nameof(SeatNumber))]
public class FlightBooking
{
    [Key]
    public required string FlightId { get; init; }
    public required string SeatNumber { get; init; }
    public required Guid UserId { get; init; }
    public required string BookingId { get; init; }
    

    public static FlightBooking Create(string FlightId, string seatNumber, Guid userId)
    {
        return new FlightBooking
        {
            FlightId = FlightId,
            SeatNumber = seatNumber,
            UserId = userId,
            BookingId = Computation.ComputeBookingId(FlightId, userId.ToString())
        };
    }
}