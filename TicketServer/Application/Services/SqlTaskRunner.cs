using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using TicketServer.Domain.Redis;
using TicketServer.Schedule;
using TicketServer.Infrastructure.Database;
using TicketServer.Api.Dto;
using System.Text.Json;

namespace TicketServer.Application.Services;
 
public class SqlTaskRunner : BackgroundService, IJobRunner<SqlTask>
{
    private readonly IDatabase _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _loadCount = 500;
    private int _startCount = 0;
    private ISqlTaskProcessor _sqlTaskProcessor;
    private readonly ILogger<SqlTaskRunner> _logger;
    public SqlTaskRunner(IConnectionMultiplexer connectionMultiplexer,
                        IServiceScopeFactory scopeFactory,
                        ILogger<SqlTaskRunner> logger,
                        ISqlTaskProcessor sqlTaskProcessor)
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
        try
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

                // process sql tasks
                foreach (var job in jobs)
                {
                    var serializedSqlTask = job.ToString();

                    var sqlTask = JsonSerializer.Deserialize<SqlTask>(serializedSqlTask);

                    if (sqlTask != null)
                    {
                        // Process the SQL task
                        await _sqlTaskProcessor.ProcessSqlTaskAsync(sqlTask, flightDbContext);
                    }


                    await _redis.SortedSetRemoveAsync(RedisKeys.SqlTaskKey, serializedSqlTask);
                }
            }
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while writing to the database in the background.");
        }
    }
}