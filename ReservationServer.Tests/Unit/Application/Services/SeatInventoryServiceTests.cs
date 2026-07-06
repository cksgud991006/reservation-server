using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ReservationServer.Application.Services;
using ReservationServer.Application.Repositories;
using ReservationServer.Application.Schedule;
using ReservationServer.Api.Dto;
using ReservationServer.Infrastructure.Redis;
using ReservationServer.Domain.Response;

public class SeatInventoryServiceTests
{
    private readonly Mock<ILogger<SeatInventoryService>> _mockLogger;
    private readonly Mock<ISeatInventoryRepository> _mockSeatInventoryRepository;
    private readonly Mock<IDatabase> _mockRedis;
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
    private readonly Mock<IRedisSession> _mockSession;
    private readonly Mock<IJobScheduler<SqlTask>> _mockJobScheduler;
    private readonly ISeatInventoryService _seatInventoryService;

    public SeatInventoryServiceTests()
    {
        _mockLogger = new Mock<ILogger<SeatInventoryService>>();
        _mockSeatInventoryRepository = new Mock<ISeatInventoryRepository>();
        _mockRedis = new Mock<IDatabase>();
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockConnectionMultiplexer.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedis.Object);
        _mockSession = new Mock<IRedisSession>();
        _mockJobScheduler = new Mock<IJobScheduler<SqlTask>>();
        _seatInventoryService = new SeatInventoryService(_mockLogger.Object, 
                                                        _mockSeatInventoryRepository.Object, 
                                                        _mockConnectionMultiplexer.Object,
                                                        _mockSession.Object,
                                                        _mockJobScheduler.Object);
    }

    [Fact]
    public async Task ReserveSeatAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var flightId = "AA123";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();

        _mockSession.Setup(r => r.GetSessionStatusAsync(It.IsAny<Guid>())).ReturnsAsync(SessionStatusResponse.Active(DateTimeOffset.Now.AddMinutes(10)));

        _mockRedis.Setup(r => r.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(RedisResult.Create(new RedisValue[] { 1, "Seat booked successfully" })); // Simulate successful reservation

        // Act
        var result = await _seatInventoryService.ReserveSeatAsync(flightId, seatNumber, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(flightId, result.FlightId);
        Assert.Equal(seatNumber, result.SeatNumber);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Seat booked successfully", result.Details);
        Assert.False(string.IsNullOrEmpty(result.BookingId));
    }

    [Fact]
    public async Task ReserveSeatAsync_InvalidPosition_ReturnsCreateFailureResponse()
    {
        // Arrange
        var flightId = "AA123";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();

        _mockSession.Setup(r => r.GetSessionStatusAsync(It.IsAny<Guid>())).ReturnsAsync(SessionStatusResponse.NotActive());

        // Act
        var result = await _seatInventoryService.ReserveSeatAsync(flightId, seatNumber, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(flightId, result.FlightId);
        Assert.Equal(seatNumber, result.SeatNumber);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("User is not in the active queue.", result.Details);
    }

    [Fact]
    public async Task ReserveSeatAsync_DuplicateRequest_ReturnsAlreadyReservedResponse()
    {
        // Arrange
        var flightId = "AA123";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();

        _mockSession.Setup(r => r.GetSessionStatusAsync(It.IsAny<Guid>())).ReturnsAsync(SessionStatusResponse.Active(DateTimeOffset.Now.AddMinutes(10)));

        _mockRedis.Setup(r => r.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(RedisResult.Create(new RedisValue[] { 0, "Seat is already occupied" })); // Simulate duplicate reservation

        // Act
        var result = await _seatInventoryService.ReserveSeatAsync(flightId, seatNumber, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(flightId, result.FlightId);
        Assert.Equal(seatNumber, result.SeatNumber);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Seat is already occupied", result.Details);
    }

    [Fact]
    public async Task ReserveSeatAsync_RunOutOfSeatRequest_ReturnsNoAvailableSeatsResponse()
    {
        // Arrange
        var flightId = "AA123";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();

        _mockSession.Setup(r => r.GetSessionStatusAsync(It.IsAny<Guid>())).ReturnsAsync(SessionStatusResponse.Active(DateTimeOffset.Now.AddMinutes(10)));

        _mockRedis.Setup(r => r.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(RedisResult.Create(new RedisValue[] { -2, "No seats available" })); // Simulate no seats available

        // Act
        var result = await _seatInventoryService.ReserveSeatAsync(flightId, seatNumber, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(flightId, result.FlightId);
        Assert.Equal(seatNumber, result.SeatNumber);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("No seats available", result.Details);
    }

    [Fact]
    public async Task ReserveSeatAsync_InvalidRequest_ThrowsException()
    {
        // Arrange
        var flightId = "AA123";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();

        _mockSession.Setup(r => r.GetSessionStatusAsync(It.IsAny<Guid>())).ReturnsAsync(SessionStatusResponse.Active(DateTimeOffset.Now.AddMinutes(10)));

        _mockRedis.Setup(r => r.ScriptEvaluateAsync(It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
                  .ReturnsAsync(RedisResult.Create(new RedisValue[] { 100, "" })); // Simulate unexpected response code

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _seatInventoryService.ReserveSeatAsync(flightId, seatNumber, userId));
    }
}
