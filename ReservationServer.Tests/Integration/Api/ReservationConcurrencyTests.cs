// ConcurrencyTests.cs
// TODO: Review this
/*
public class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ConcurrencyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConcurrentReservationRequests_OnlyOneSucceeds()
    {
        // Arrange: 동일한 방/시간대로 100개의 동시 요청 준비
        var request = new
        {
            RoomId = 99,
            StartDate = "2026-10-01",
            EndDate = "2026-10-03"
        };

        var tasks = new List<Task<HttpResponseMessage>>();

        // Act: 100개 요청을 동시에 발사
        for (int i = 0; i < 100; i++)
        {
            var client = _factory.CreateClient();
            tasks.Add(client.PostAsJsonAsync("/api/reservations", request));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert: 정확히 1개만 성공(201), 나머지는 실패(409)
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, successCount);
        Assert.Equal(99, conflictCount);
    }
}
*/