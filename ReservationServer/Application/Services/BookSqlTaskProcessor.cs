using ReservationServer.Api.Dto;
using ReservationServer.Infrastructure.Database;
using ReservationServer.Domain.Database;

namespace ReservationServer.Application.Services;

public class BookSqlTaskProcessor : IDbTaskProcessor
{
    private readonly ILogger<IDbTaskProcessor> _logger;

    public BookSqlTaskProcessor(ILogger<IDbTaskProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessTaskAsync(SqlTask sqlTask, FlightDbContext flightDbContext)
    {
        // Implementation for processing the SQL task
        // This is where you would interact with the FlightDbContext to perform the necessary operations
        flightDbContext.FlightBookings.Add(FlightBooking.Create(
            sqlTask.FlightId,
            sqlTask.SeatNumber,
            sqlTask.UserId
        ));
        
        await flightDbContext.SaveChangesAsync();

        _logger.LogInformation("Processed SQL task. FlightId: {FlightId}, SeatNumber: {SeatNumber}, UserId: {UserId}",
            sqlTask.FlightId, sqlTask.SeatNumber, sqlTask.UserId);
    }
}