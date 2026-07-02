using TicketServer.Application.Repositories;
using TicketServer.Domain.Redis;
using StackExchange.Redis;
using TicketServer.Domain.Response;
using TicketServer.Core;
using TicketServer.Api.Dto;
using TicketServer.Application.Schedule;

namespace TicketServer.Application.Services;
public class SeatInventoryService : ISeatInventoryService
{
    private readonly ILogger<ISeatInventoryService> _logger;
    private readonly ISeatInventoryRepository _seatInventoryRepository;
    private readonly IDatabase _redis;
    private readonly IJobScheduler<SqlTask> _jobScheduler;

    public SeatInventoryService(ILogger<ISeatInventoryService> logger,
                              ISeatInventoryRepository seatInventoryRepository,
                              IConnectionMultiplexer connectionMultiplexer,
                              IJobScheduler<SqlTask> jobScheduler)
    {
        _logger = logger;           
        _seatInventoryRepository = seatInventoryRepository;
        _redis = connectionMultiplexer.GetDatabase();
        _jobScheduler = jobScheduler;
    }

    public async Task<SeatInventoryResponse> ReserveSeatAsync(string flightId, string seatNumber, Guid id)
    {
        _logger.LogInformation("Received seat reservation request. FlightId: {FlightId}, SeatNumber: {SeatNumber}, UserId: {UserId}",
            flightId, seatNumber, id);
        var pos = await _redis.SortedSetRankAsync(RedisKeys.QueueActiveKey, id.ToString());
        if (pos == null) {
            return SeatInventoryResponse.CreateFailureResponse(flightId, seatNumber, id, "User is not in the active queue.");
        }

        var flightNumber = Computation.ComputeFlightNumberByFlightId(flightId);
        var departureTime = Computation.ComputeDepartureTimeByFlightId(flightId);

        _logger.LogInformation("Computed flight number and departure time. FlightNumber: {FlightNumber}, DepartureTime: {DepartureTime}",
            flightNumber, departureTime);

        var flightInstanceKey = RedisKeys.FlightInstance(flightNumber, Format.FormatDate(departureTime));
        var flightSeatCountKey = RedisKeys.FlightSeatCount(flightId);
        var seatLayoutKey = RedisKeys.SeatLayout(flightNumber, seatNumber);
        var flightBookingKey = RedisKeys.FlightBooking(flightId, seatNumber);

        _logger.LogInformation("Attempting to reserve seat. FlightInstanceKey: {FlightInstanceKey}, FlightSeatCountKey: {FlightSeatCountKey}, SeatLayoutKey: {SeatLayoutKey}, FlightBookingKey: {FlightBookingKey}, UserId: {UserId}",
            flightInstanceKey, flightSeatCountKey, seatLayoutKey, flightBookingKey, id);

        var result = await _redis.ScriptEvaluateAsync(
            RedisLuaScripts.ReserveSeatScript.ExecutableScript,
            [flightInstanceKey, flightSeatCountKey, seatLayoutKey, flightBookingKey],
            [id.ToString()]);
        
        var data = (RedisResult[]) result!;

        int ResponseCode = (int)data[0];
        string details = (string)data[1]!;
        string bookingId = Computation.ComputeBookingId(flightId, id.ToString());

        switch (ResponseCode)
        {
            case 0:
                return SeatInventoryResponse.AlreadyReservedResponse(flightId, seatNumber, id, bookingId, details);
            case 1:
                
                // TODO: create db task to update db in background, and implement retry logic for failed db updates
                /*
                var seat = await _seatInventoryRepository.GetSeat(flightNumber, classType, seatNumber);
                await _seatRepository.UpdateSeatStatus(seat!, SeatStatus.Held, id.ToString());
                */
                var sqlTask = new SqlTask
                (
                    SqlTaskType.BookSeat,
                    flightId,
                    seatNumber,
                    id
                );

                var currentTime = DateTimeOffset.UtcNow;

                await _jobScheduler.ScheduleAsync(sqlTask, currentTime); // run background job to update db asynchronously
                return SeatInventoryResponse.CreateSuccessResponse(flightId, seatNumber, id, bookingId, details);
            case -1:
                return SeatInventoryResponse.CreateFailureResponse(flightId, seatNumber, id, details);
            case -2:
                return SeatInventoryResponse.NoAvailableSeatsResponse(flightId, seatNumber, id, details);
            default:
                throw new InvalidOperationException("Unexpected Response code from Redis script.");
        }
    }
}