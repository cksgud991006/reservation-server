using ReservationServer.Domain.Database;

namespace ReservationServer.Application.Repositories;

public interface ISeatInventoryRepository
{     
    Task<FlightInstance?> GetFlightInstance(string flightNumber, string departureTime);
    Task<List<FlightInstance>> GetFlightInstances();
    Task<FlightSeatCount?> GetFlightSeatCount(string flightId);
    Task<List<FlightSeatCount>> GetFlightSeatCounts();
    Task<SeatLayout?> GetSeatLayout(string flightNumber, string seatNumber);
    Task<List<SeatLayout>> GetSeatLayout(string flightNumber);
    Task<List<SeatLayout>> GetSeatLayout();
    Task<List<FlightBooking>> GetFlightBookings(string flightId);
    Task<List<FlightBooking>> GetFlightBookings();
    Task<FlightBooking?> GetBookedStatusAsync(string bookingId);
    Task<FlightBooking?> GetBookStatusAsync(Guid userId);
    Task<int> GetUnavailableSeats(string flightId);
    Task<int> GetAvailableSeats(string flightId);
    Task SetTotalSeatCount(string flightId, int newTotalSeats);
    Task AddFlightInstance(FlightInstance flightInstance);
    Task AddFlightSeatCount(FlightSeatCount flightSeatCount);
    Task AddSeatLayout(SeatLayout seatLayout);
    Task AddBooking(string flightId, string seatNumber, Guid userId);
}