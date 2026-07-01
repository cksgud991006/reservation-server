namespace TicketServer.Application.BackgroundJobs;

public interface IJobRunner<T>
{
    Task RunTask();
}