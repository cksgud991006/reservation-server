using ReservationServer.Application.Repositories;
using ReservationServer.Domain.Redis;
using StackExchange.Redis;
using ReservationServer.Domain.Response;
using ReservationServer.Core;
using ReservationServer.Api.Dto;
using ReservationServer.Application.Schedule;

namespace ReservationServer.Application.Services;
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
        
        // Check if the user is in the session
        var pos = await _redis.SortedSetRankAsync(RedisKeys.QueueActiveKey, id.ToString());
        if (pos == null) {
            return SeatInventoryResponse.CreateFailureResponse(flightId, seatNumber, id, "User is not in the active queue.");
        }

        // Compute Redis keys
        var flightNumber = Computation.ComputeFlightNumberByFlightId(flightId);
        var departureTime = Computation.ComputeDepartureTimeByFlightId(flightId);
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

        switch (ResponseCode)
        {
            case 0:
                return SeatInventoryResponse.AlreadyReservedResponse(flightId, seatNumber, id, details);
            case 1:
                var sqlTask = SqlTask.CreateBookSeatTask(flightId, seatNumber, id);

                var currentTime = DateTimeOffset.UtcNow;

                await _jobScheduler.ScheduleAsync(sqlTask, currentTime); // run background job to update db asynchronously
                return SeatInventoryResponse.CreateSuccessResponse(flightId, seatNumber, id, sqlTask.BookingId, details);
            case -1:
                return SeatInventoryResponse.CreateFailureResponse(flightId, seatNumber, id, details);
            case -2:
                return SeatInventoryResponse.NoAvailableSeatsResponse(flightId, seatNumber, id, details);
            default:
                throw new InvalidOperationException("Unexpected Response code from Redis script.");
        }
    }
}