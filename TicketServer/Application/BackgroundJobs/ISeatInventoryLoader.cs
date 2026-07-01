namespace TicketServer.Application.BackgroundJobs;

public interface ISeatInventoryLoader : IHostedService
{
    // This interface is intentionally left blank as a marker for the hosted service
    // This interface set up NoSQL data in Redis loaded from SQL database at application startup
}