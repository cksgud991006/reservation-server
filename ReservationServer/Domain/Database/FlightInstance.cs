using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ReservationServer.Domain.Database;    

[PrimaryKey(nameof(FlightNumber), nameof(DepartureTime))]
public class FlightInstance
{
    [Key]
    public required string FlightNumber { get; init; } 
    public required string DepartureTime { get; init; }
    public required string FlightId { get; init; }

    public static FlightInstance Create(string flightNumber, string departureTime, string FlightId)
    {
        return new FlightInstance
        {
            FlightNumber = flightNumber,
            DepartureTime = departureTime,
            FlightId = FlightId
        };
    }
}