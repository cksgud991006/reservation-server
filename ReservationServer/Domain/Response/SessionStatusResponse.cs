namespace ReservationServer.Domain.Response;
public class SessionStatusResponse
{
    public bool IsActive { get; private set; }
    public DateTimeOffset TimeExpiry { get; private set; }

    private SessionStatusResponse(bool isActive, DateTimeOffset timeExpiry)
    {
        IsActive = isActive;
        TimeExpiry = timeExpiry;
    }

    public static SessionStatusResponse Active(DateTimeOffset timeExpiry) =>
        new SessionStatusResponse(true, timeExpiry);    

    public static SessionStatusResponse NotActive() =>
        new SessionStatusResponse(false, DateTimeOffset.MinValue);
}