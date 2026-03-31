using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketServer.Domain.Seats;
using TicketServer.Infrastructure.Database;
using TicketServer.Domain.Seed;

namespace TicketServer.Application.Repositories;

public class DbInitializer(IServiceScopeFactory scopeFactory, 
                           IOptions<SeedDataOptions> seedDataOptions,
                           ILogger<DbInitializer> logger): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            var seatContext = scope.ServiceProvider.GetRequiredService<SeatContext>();
            
            seatContext.Database.Migrate();

            foreach (var config in seedDataOptions.Value.Flights)
            {
                // 1. Check/Add Flight Inventory
                var inventory = await seatContext.FlightInventories
                    .FirstOrDefaultAsync(f => f.FlightNumber == config.FlightNumber);

                if (inventory == null)
                {
                    inventory = new FlightInventory
                    {
                        FlightNumber = config.FlightNumber,
                        TotalSeats = config.SeatCount,
                        AvailableSeats = config.SeatCount
                    };
                    seatContext.FlightInventories.Add(inventory);
                }

                // 2. Check/Add Seats for this flight
                var existingSeatsCount = await seatContext.Seats
                    .CountAsync(s => s.FlightNumber == config.FlightNumber);
                if (existingSeatsCount < config.SeatCount)
                {
                    var seatsToAdd = new List<Seat>();
                    for (int i = existingSeatsCount; i < config.SeatCount; ++i)
                    {
                        seatsToAdd.Add(Seat.Create(config.FlightNumber, ClassType.Economy, $"{config.Prefix}{i}", SeatStatus.Available));
                    }

                    seatContext.Seats.AddRange(seatsToAdd);
                }
            }
            
            await seatContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Implementation for stopping the database initializer if needed
    }
}