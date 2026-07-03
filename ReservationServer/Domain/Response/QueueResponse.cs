namespace ReservationServer.Domain.Response;
public class QueueResponse
{
    public bool IsInQueue { get; private set; }
    public int Position { get; private set; }

    private QueueResponse(bool isInQueue, int position)
    {
        IsInQueue = isInQueue;
        Position = position;
    }

    public static QueueResponse InQueue(int position) =>
        new QueueResponse(true, position);    

    public static QueueResponse NotInQueue() =>
        new QueueResponse(false, -1);
}