using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketServer.Domain.Database;
using TicketServer.Infrastructure.Database;
using TicketServer.Domain.Seed;
using TicketServer.Core;

namespace TicketServer.Application.BackgroundJobs;

public class DbInitializer(IServiceScopeFactory scopeFactory, 
                           IConfiguration configuration,
                           ILogger<DbInitializer> logger): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            var flightDbContext = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
            
            await flightDbContext.Database.MigrateAsync(cancellationToken);

            var options = configuration.GetSection("SeedData").Get<SeedDataOptions>();

            foreach (var config in options.Flights)
            {

                string time = Format.FormatDate(config.DepartureTime);

                if (time == null)
                {
                    logger.LogError("Invalid departure time format for flight {FlightNumber}: {DepartureTime}", config.FlightNumber, config.DepartureTime);
                    continue; // Skip this flight and move to the next one
                }

                var flightId = Computation.ComputeFlightId(config.FlightNumber, time);

                var inventory = await flightDbContext.FlightSeatCounts
                    .FirstOrDefaultAsync(f => f.FlightId == flightId);

                if (inventory == null)
                {
                    // 1. Check/Add Flight
                    inventory = new FlightSeatCount
                    {
                        FlightId = flightId,
                        TotalSeatCount = config.SeatCount,
                    };
                    flightDbContext.FlightSeatCounts.Add(inventory);
    
                    // 2. Check/Add Seat Layout for the flight
                    var seatsToAdd = new List<SeatLayout>();
                    for (int i = 0; i < config.SeatCount; ++i)
                    {
                        seatsToAdd.Add(SeatLayout.Create(config.FlightNumber, ClassType.Economy, $"{i}{config.Prefix}"));
                    }

                    flightDbContext.SeatLayouts.AddRange(seatsToAdd);

                    // 3. Check/Add Flight Instance
                    flightDbContext.FlightInstances.Add(new FlightInstance
                    {
                        FlightId = flightId,
                        DepartureTime = time,
                        FlightNumber = config.FlightNumber
                    });
                }
            }
            
            await flightDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Implementation for stopping the database initializer if needed
    }
}