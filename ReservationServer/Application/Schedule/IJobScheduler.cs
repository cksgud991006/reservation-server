namespace ReservationServer.Application.Schedule;

public interface IJobScheduler<T>
{
    Task<int> GetWaitingPositionAsync(T id);
    Task ScheduleAsync(T payload, DateTimeOffset scheduleTime);
}