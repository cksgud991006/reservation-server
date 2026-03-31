namespace TicketServer.Domain.Seed;
public record SeedDataOptions
{
    public List<FlightSeed> Flights { get; set; } = new();
}

public record FlightSeed
{
    public string FlightNumber { get; set; }
    public int SeatCount { get; set; }
    public string Prefix { get; set; }
}