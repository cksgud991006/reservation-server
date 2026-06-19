using TicketServer.Api.Dto;
using TicketServer.Infrastructure.Database;
using TicketServer.Domain.Database;

namespace TicketServer.Application.Services;

public class BookSqlTaskProcessor : ISqlTaskProcessor
{
    public async Task ProcessSqlTaskAsync(SqlTask sqlTask, FlightDbContext flightDbContext)
    {
        // Implementation for processing the SQL task
        // This is where you would interact with the FlightDbContext to perform the necessary operations
        flightDbContext.FlightBookings.Add(new FlightBooking
        {
            FlightId = sqlTask.FlightId,
            SeatNumber = sqlTask.SeatNumber,
            UserId = sqlTask.UserId
        });
        
        await flightDbContext.SaveChangesAsync();
    }
}