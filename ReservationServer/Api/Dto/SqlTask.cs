using System.ComponentModel.DataAnnotations;
using ReservationServer.Core;

namespace ReservationServer.Api.Dto;

public enum SqlTaskType
{
    BookSeat,
    CancelSeat
}
public class SqlTask
{
    public SqlTask(SqlTaskType type, string flightId, string seatNumber, Guid userId, string bookingId)
    {
        Type = type;
        FlightId = flightId;
        SeatNumber = seatNumber;
        UserId = userId;
        BookingId = bookingId;
    }

    public SqlTaskType Type { get; }
    public string FlightId { get; }
    public string SeatNumber { get; }
    public Guid UserId { get; }
    public string BookingId { get; }

    public static SqlTask CreateBookSeatTask(string flightId, string seatNumber, Guid userId)
    {
        return new SqlTask(
            SqlTaskType.BookSeat, 
            flightId, 
            seatNumber, 
            userId, 
            Computation.ComputeBookingId(flightId, userId.ToString())
        );
    }
};