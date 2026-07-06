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
    private readonly TimeSpan _minDelayMs = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _maxDelayS = TimeSpan.FromSeconds(10);
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
            int processed = await RunTask();

            var delay = processed >= _loadCount ? _minDelayMs   // batch full -> set to drain fast
                        : processed == 0 ?  _maxDelayS          // revert delay
                        : TimeSpan.FromSeconds(2);              // default delay

            await Task.Delay(delay, stoppingToken);
        }
    }

    public async Task<int> RunTask()
    {
        
        // 1. Create a scope to resolve scoped services like DbContext
        using var scope = _scopeFactory.CreateScope();

        var flightDbContext = scope.ServiceProvider.GetRequiredService<FlightDbContext>();

        var jobs = await _redis.SortedSetRangeByRankAsync(
                RedisKeys.SqlTaskKey,
                0,
                -1,
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

        return jobs.Length;
    }
}