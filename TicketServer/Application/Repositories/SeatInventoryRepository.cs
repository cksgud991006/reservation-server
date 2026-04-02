using Microsoft.EntityFrameworkCore;
using TicketServer.Domain.Seats;
using TicketServer.Infrastructure.Database;

namespace TicketServer.Application.Repositories;

public class SeatInventoryRepository : ISeatInventoryRepository
{
    private readonly ILogger<SeatInventoryRepository> _logger;
    private readonly SeatContext _context;

    public SeatInventoryRepository(ILogger<SeatInventoryRepository> logger, SeatContext context)
    {
        _logger = logger;
        _context = context;
    }

    public Task<int> GetTotalSeats(string flightNumber)
    {
        return _context.FlightInventories
            .Where(f => f.FlightNumber == flightNumber)
            .Select(f => f.TotalSeats)
            .FirstOrDefaultAsync();
    }
    public Task<int> GetAvailableSeats(string flightNumber)
    {
        return _context.FlightInventories
            .Where(f => f.FlightNumber == flightNumber)
            .Select(f => f.AvailableSeats)
            .FirstOrDefaultAsync();
    }   
    
    public Task<Seat?> GetSeat(string flightNumber, ClassType classType, string seatNumber)
    {
        return _context.Seats.FirstOrDefaultAsync(s =>
            s.FlightNumber == flightNumber &&
            s.SeatClass == classType &&
            s.SeatNumber == seatNumber);
    }

    public Task<List<Seat>> GetSeats()
    {
        return _context.Seats.ToListAsync();
    }

    public Task UpdateTotalSeats(string flightNumber, int newTotalSeats)
    {
        _context.FlightInventories
            .Where(f => f.FlightNumber == flightNumber)
            .ExecuteUpdateAsync(row => row.SetProperty(f => f.TotalSeats, newTotalSeats));
        
        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }
    public Task UpdateAvailableSeats(string flightNumber, int newAvailableSeats)
    {

        _context.FlightInventories
            .Where(f => f.FlightNumber == flightNumber)
            .ExecuteUpdateAsync(row => row.SetProperty(f => f.AvailableSeats, newAvailableSeats));

        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }
    public async Task UpdateSeatStatus(Seat seat, SeatStatus newStatus, string heldByUserId)
    {
        int affectedRows = await _context.Seats
                .Where(s => s.SeatId == seat.SeatId && s.Status == SeatStatus.Available)
                .ExecuteUpdateAsync(row => row
                    .SetProperty(s => s.Status, newStatus)
                    .SetProperty(s => s.HeldByUserId, heldByUserId));
 
        // 2. Logic Check: If 0, the seat wasn't available (or ID was wrong)
        if (affectedRows == 0)
        {
            // Throw a specific error that your controller can catch
            throw new InvalidOperationException("SEAT_UNAVAILABLE");
        }
    }
}