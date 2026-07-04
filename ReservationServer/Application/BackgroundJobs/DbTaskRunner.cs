using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using ReservationServer.Domain.Redis;
using ReservationServer.Application.Schedule;
using ReservationServer.Application.Services;
using ReservationServer.Infrastructure.Database;
using ReservationServer.Api.Dto;
using System.Text.Json;

namespace ReservationServer.Application.BackgroundJobs;
 
public class DbTaskRunner : BackgroundService, IJobRunner<SqlTask>
{
    private readonly IDatabase _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _loadCount = 500;
    private int _startCount = 0;
    private IDbTaskProcessor _sqlTaskProcessor;
    private readonly ILogger<DbTaskRunner> _logger;
    public DbTaskRunner(IConnectionMultiplexer connectionMultiplexer,
                        IServiceScopeFactory scopeFactory,
                        ILogger<DbTaskRunner> logger,
                        IDbTaskProcessor sqlTaskProcessor)
    {
        _redis = connectionMultiplexer.GetDatabase();
        _scopeFactory = scopeFactory;
        _logger = logger;
        _sqlTaskProcessor = sqlTaskProcessor;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation("SQL task worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunTask();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public async Task RunTask()
    {
        
        // 1. Create a scope to resolve scoped services like DbContext
        using (var scope = _scopeFactory.CreateScope())
        {

            var flightDbContext = scope.ServiceProvider.GetRequiredService<FlightDbContext>();

            var jobs = await _redis.SortedSetRangeByRankAsync(
                    RedisKeys.SqlTaskKey,
                    _startCount,
                    _loadCount,
                    Order.Ascending);

            // Flush sql tasks
            foreach (var job in jobs)
            {
                var serializedSqlTask = job.ToString();

                await _redis.SortedSetRemoveAsync(RedisKeys.SqlTaskKey, serializedSqlTask);

                var sqlTask = JsonSerializer.Deserialize<SqlTask>(serializedSqlTask);

                // Process the SQL task
                await _sqlTaskProcessor.ProcessTaskAsync(sqlTask, flightDbContext);
            }

            if (jobs.Length > 0)
            {
                try
                {
                    await flightDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving changes to the database.");
                }
            }
        }
    }
}