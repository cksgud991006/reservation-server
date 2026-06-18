using System.ComponentModel.DataAnnotations;

namespace TicketServer.Domain.Database;

public class FlightSeatCount
{
    [Key]
    public required string FlightId { get; set; }
    public int TotalSeatCount { get; set; }

    public static FlightSeatCount Create(string FlightId, int totalSeatCount)
    {
        return new FlightSeatCount
        {
            FlightId = FlightId,
            TotalSeatCount = totalSeatCount
        };
    }
}