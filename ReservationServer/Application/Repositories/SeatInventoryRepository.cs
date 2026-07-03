using Microsoft.EntityFrameworkCore;
using ReservationServer.Domain.Database;
using ReservationServer.Infrastructure.Database;

namespace ReservationServer.Application.Repositories;

public class SeatInventoryRepository : ISeatInventoryRepository
{
    private readonly ILogger<SeatInventoryRepository> _logger;
    private readonly FlightDbContext _context;

    public SeatInventoryRepository(ILogger<SeatInventoryRepository> logger, FlightDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public Task<FlightInstance?> GetFlightInstance(string flightNumber, string departureTime)
    {
        return _context.FlightInstances
            .Where(s => s.FlightNumber == flightNumber && s.DepartureTime == departureTime)
            .FirstOrDefaultAsync();
    }

    public Task<List<FlightInstance>> GetFlightInstances()
    {
        return _context.FlightInstances
            .ToListAsync();
    }

    public Task<FlightSeatCount?> GetFlightSeatCount(string flightId)
    {
        return _context.FlightSeatCounts
            .Where(s => s.FlightId == flightId)
            .FirstOrDefaultAsync();
    }

    public Task<List<FlightSeatCount>> GetFlightSeatCounts()
    {
        return _context.FlightSeatCounts
            .ToListAsync();
    }

    public Task<SeatLayout?> GetSeatLayout(string flightNumber, string seatNumber)
    {
        return _context.SeatLayouts.FirstOrDefaultAsync(s  =>
            s.FlightNumber == flightNumber &&
            s.SeatNumber == seatNumber);
    }

    public Task<List<SeatLayout>> GetSeatLayout(string flightNumber)
    {
        return _context.SeatLayouts
            .Where(s  => s.FlightNumber == flightNumber)
            .ToListAsync();
    }

    public Task<List<SeatLayout>> GetSeatLayout()
    {
        return _context.SeatLayouts.ToListAsync();
    }

    public Task<List<FlightBooking>> GetFlightBookings(string flightId)
    {
        return _context.FlightBookings.Where(s => 
            s.FlightId == flightId)
            .ToListAsync();
    }

    public Task<List<FlightBooking>> GetFlightBookings()
    {
        return _context.FlightBookings.ToListAsync();
    }

    public Task<FlightBooking?> GetBookedStatusAsync(string bookingId)
    {
        return _context.FlightBookings.FirstOrDefaultAsync(s => 
            s.BookingId == bookingId);
    }

    public Task<FlightBooking?> GetBookStatusAsync(Guid userId)
    {
        return _context.FlightBookings.FirstOrDefaultAsync(s => 
            s.UserId == userId);
    }

    public Task<int> GetUnavailableSeats(string flightId)
    {
        return _context.FlightBookings
            .Where(f => f.FlightId == flightId)
            .CountAsync();
    }

    public Task<int> GetAvailableSeats(string flightId)
    {

        FlightSeatCount? flightSeatCount = GetFlightSeatCount(flightId).Result;

        int totalSeats = flightSeatCount?.TotalSeatCount ?? 0;

        int unavailableSeats = GetUnavailableSeats(flightId).Result;

        return totalSeats - unavailableSeats >= 0 ? Task.FromResult(totalSeats - unavailableSeats) : Task.FromResult(0);
    }   
    
    public Task SetTotalSeatCount(string flightId, int newTotalSeats)
    {
        _context.FlightSeatCounts
            .Where(f => f.FlightId == flightId)
            .ExecuteUpdateAsync(row => row.SetProperty(f => f.TotalSeatCount, newTotalSeats));
        
        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }


    public Task AddFlightInstance(FlightInstance flightInstance)
    {
        _context.FlightInstances.Add(flightInstance);
        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }

    public Task AddFlightSeatCount(FlightSeatCount flightSeatCount)
    {
        _context.FlightSeatCounts.Add(flightSeatCount);
        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }

    public Task AddSeatLayout(SeatLayout seatLayout)
    {
        _context.SeatLayouts.Add(seatLayout);
        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }
    
    public Task AddBooking(string flightId, string seatNumber, Guid userId)
    {
        var booking = FlightBooking.Create(
            flightId,
            seatNumber,
            userId
        );

        _context.FlightBookings.Add(booking);
        _context.SaveChangesAsync();

        return Task.CompletedTask;
    }
}