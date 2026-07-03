namespace ReservationServer.Domain.Seed;
public record SeedDataOptions
{
    public List<FlightSeed> Flights { get; set; } = new();
}

public record FlightSeed
{
    public required string FlightNumber { get; set; }
    public required string DepartureTime { get; set; }
    public required string FlightId { get; set; }
    public required int SeatCount { get; set; }
    public required string Prefix { get; set; }
}