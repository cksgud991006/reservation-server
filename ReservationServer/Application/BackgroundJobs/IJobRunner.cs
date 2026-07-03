namespace ReservationServer.Application.BackgroundJobs;

public interface IJobRunner<T>
{
    Task RunTask();
}