using TicketServer.Domain.Database;

namespace TicketServer.Application.Repositories;

public interface ISeatInventoryRepository
{     
    Task<FlightInstance?> GetFlightInstance(string flightNumber, DateTime departureTime);
     Task<List<FlightInstance>> GetFlightInstances();
     Task<FlightSeatCount?> GetFlightSeatCount(string flightId);
     Task<List<FlightSeatCount>> GetFlightSeatCounts();
     Task<SeatLayout?> GetSeatLayout(string flightNumber, string seatNumber);
     Task<List<SeatLayout>> GetSeatLayouts();
     Task<FlightBooking?> GetFlightBooking(string flightId);
     Task<List<FlightBooking>> GetFlightBookings();
     Task<int> GetUnavailableSeats(string flightId);
     Task<int> GetAvailableSeats(string flightId);
     Task SetTotalSeatCount(string flightId, int newTotalSeats);
     Task AddFlightInstance(FlightInstance flightInstance);
     Task AddFlightSeatCount(FlightSeatCount flightSeatCount);
     Task AddSeatLayout(SeatLayout seatLayout);
     Task AddBooking(string flightId, string seatNumber, string userId);
}