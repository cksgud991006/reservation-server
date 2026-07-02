using Xunit;
using Moq;
using TicketServer.Application.Services;
using TicketServer.Application.Schedule;
using TicketServer.Domain.Response;

public class QueueServiceTests
{
    private readonly Mock<IJobScheduler<Guid>> _mockJobScheduler;
    private readonly QueueService _queueService;

    public QueueServiceTests()
    {
        _mockJobScheduler = new Mock<IJobScheduler<Guid>>();
        _queueService = new QueueService(_mockJobScheduler.Object);
    }

    [Fact]
    public async Task GetPositionInQueueAsync_InQueue_ReturnsInQueueResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var position = 5;
        _mockJobScheduler.Setup(js => js.GetWaitingPositionAsync(id)).ReturnsAsync(position);

        // Act
        var result = await _queueService.GetPositionInQueueAsync(id);

        // Assert
        Assert.True(result.IsInQueue);
        Assert.Equal(position, result.Position);
    }

    [Fact]
    public async Task GetPositionInQueueAsync_NotInQueue_ReturnsNotInQueueResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockJobScheduler.Setup(js => js.GetWaitingPositionAsync(id)).ReturnsAsync(-1);

        // Act
        var result = await _queueService.GetPositionInQueueAsync(id);

        // Assert
        Assert.False(result.IsInQueue);
    }
}