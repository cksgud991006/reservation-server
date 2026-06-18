
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace TicketServer.Domain.Database;    

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClassType
{
    Economy,
    Business,
    First
}

[PrimaryKey(nameof(FlightNumber), nameof(SeatNumber))]
public class SeatLayout
{
    public required string FlightNumber { get; init; }
    public required string SeatNumber { get; init; }
    public ClassType SeatClass { get; init; }

    public static SeatLayout Create(string flightNumber, ClassType seatClass, string seatNumber)
    {
        return new SeatLayout
        {
            FlightNumber = flightNumber,
            SeatClass = seatClass,
            SeatNumber = seatNumber
        };
    }
}