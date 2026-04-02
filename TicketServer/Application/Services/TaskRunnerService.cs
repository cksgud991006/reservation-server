
using TicketServer.Schedule;

namespace TicketServer.Application.Services;
 
public class TaskRunnerService : BackgroundService
{
    private readonly IJobRunner _jobRunner;
    private readonly ILogger<TaskRunnerService> _logger;
    public TaskRunnerService(IJobRunner jobRunner,
                             ILogger<TaskRunnerService> logger)
    {
        _jobRunner = jobRunner;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _jobRunner.RunAsync();
            await Task.Delay(250, stoppingToken);
        }
    }
}