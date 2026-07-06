using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using ReservationServer.Infrastructure.Database;
using ReservationServer.Domain.Database;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test-reservation-db")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
        .WithPortBinding(6379, true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FlightDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a new DbContext registration with the test container's connection string
            services.AddDbContext<FlightDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Program.cs registers IConnectionMultiplexer as an already-connected instance,
            // built eagerly from config before the host is built — so overriding config
            // has no effect. Replace the service registration itself instead, same as above.
            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
            {
                services.Remove(redisDescriptor);
            }

            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect($"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort()}"));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();

        // Seed test data directly, using our own DbContext — NOT Services.GetRequiredService —
        // so this runs before the host (and DbInitializer/RedisInitializer) ever starts.
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        await using var seedContext = new FlightDbContext(options);

        await seedContext.Database.MigrateAsync();

        seedContext.FlightBookings.Add(
            FlightBooking.Create("AA123-2023-10-01T10:00:00Z", "12A", Guid.Parse("123e4567-e89b-12d3-a456-426614174000"))
        );

        await seedContext.SaveChangesAsync();

        // Seed Redis data directly, using our own ConnectionMultiplexer — NOT Services.GetRequiredService —
        var redis = ConnectionMultiplexer.Connect($"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort()}").GetDatabase();
        await redis.SortedSetAddAsync("queue:waiting:zset", "123e4567-e89b-12d3-a456-426614174001", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return DisposeAsync().AsTask();
    }
}