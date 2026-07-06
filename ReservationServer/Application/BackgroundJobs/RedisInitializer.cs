using ReservationServer.Application.Repositories;
using ReservationServer.Domain.Redis;
using StackExchange.Redis;

namespace ReservationServer.Application.Services;

public class RedisInitializer(IServiceScopeFactory scopeFactory, ILogger<RedisInitializer> logger): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            logger.LogInformation("Starting Redis initialization...");

            var seatInventoryRepository = scope.ServiceProvider.GetRequiredService<ISeatInventoryRepository>();
            var multiplexer = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            var redis = multiplexer.GetDatabase();
            
            var flightInstances = await seatInventoryRepository.GetFlightInstances();

            // Load flight instances into Redis
            foreach (var instance in flightInstances)
            {
                var key = RedisKeys.FlightInstance(instance.FlightNumber, instance.DepartureTime);
                var val = instance.FlightId;
                await redis.StringSetAsync(key, val).WaitAsync(cancellationToken);
            }

            // Load flight seat counts into Redis
            var seatCounts = await seatInventoryRepository.GetFlightSeatCounts();
            foreach (var seatCount in seatCounts)
            {
                var key = RedisKeys.FlightSeatCount(seatCount.FlightId);
                var val = seatCount.TotalSeatCount;
                await redis.StringSetAsync(key, val).WaitAsync(cancellationToken);
            }

            // Load seat layouts into Redis
            var seatLayouts = await seatInventoryRepository.GetSeatLayout();
            foreach (var layout in seatLayouts)
            {
                var key = RedisKeys.SeatLayout(layout.FlightNumber, layout.SeatNumber);
                var val = layout.SeatClass.ToString();
                await redis.StringSetAsync(key, val).WaitAsync(cancellationToken);
            }

            // Load flight bookings into Redis
            var bookings = await seatInventoryRepository.GetFlightBookings();
            foreach (var booking in bookings)
            {
                var key = RedisKeys.FlightBooking(booking.FlightId, booking.SeatNumber);
                var val = booking.UserId.ToString();
                await redis.SetAddAsync(key, val).WaitAsync(cancellationToken);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Implementation for stopping the inventory loader if needed
    }
}