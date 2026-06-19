using Microsoft.EntityFrameworkCore;
using TicketServer.Domain.Database;

namespace TicketServer.Infrastructure.Database;
public class FlightDbContext: DbContext
{
    public DbSet<FlightBooking> FlightBookings { get; set; }
    public DbSet<FlightInstance> FlightInstances { get; set; }
    public DbSet<FlightSeatCount> FlightSeatCounts { get; set; }
    public DbSet<SeatLayout> SeatLayouts { get; set; }

    public FlightDbContext(DbContextOptions<FlightDbContext> options) 
    : base(options)
    {
    }
}