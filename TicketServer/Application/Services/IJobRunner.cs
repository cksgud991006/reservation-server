namespace TicketServer.Application.Services;

public interface IJobRunner<T>
{
    Task RunTask();
}