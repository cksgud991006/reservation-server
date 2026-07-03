
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

public class ReservationApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ReservationApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Flights
    [Fact]
    public async Task GetFlightInstances_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/flights");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightInstance_ReturnsOk()
    {
        // Act: Do url encoding for query parameters
        var flightNumber = "AA123";
        var departureTime = "2023-10-01T10:00:00Z";
        var url = $"/api/flights/{flightNumber}/{departureTime}";
        var response = await _client.GetAsync(url);

        // Assert: HTTP response status code is NotFound
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightInstance_ReturnsNotFound()
    {
        // Act: Do url encoding for query parameters
        var flightNumber = "AA";
        var departureTime = "2023-10-01T10:00:00Z";
        var url = $"/api/flights/{flightNumber}/{departureTime}";
        var response = await _client.GetAsync(url);

        // Assert: HTTP response status code is NotFound
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSeatLayout_ReturnsOk()
    {
        // Act
        var flightNumber = "AA123";
        var response = await _client.GetAsync($"/api/flights/{flightNumber}/seats");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightSeatCount_ReturnsOk()
    {
        // Act
        var flightId = "AA123-2023-10-01T10:00:00Z";
        var response = await _client.GetAsync($"/api/flights/{flightId}/seats/count");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightSeatCount_ReturnsNotFound()
    {
        // Act
        var flightId = "AA-2023-10-01T10:00:00Z";
        var response = await _client.GetAsync($"/api/flights/{flightId}/seats/count");

        // Assert: HTTP response status code is NotFound
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightBookings_ReturnsOk()
    {
        // Act
        var flightId = "AA123-2023-10-01T10:00:00Z";
        var response = await _client.GetAsync($"/api/flights/{flightId}/bookings");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightBooking_ReturnsOk()
    {
        // Act
        var bookingId = "AA123-2023-10-01T10:00:00Z-12A-123e4567-e89b-12d3-a456-426614174000";
        var response = await _client.GetAsync($"/api/bookings/{bookingId}");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFlightBooking_ReturnsNotFound()
    {
        // Act
        var bookingId = "AA123-2023-10-01T10:00:00Z-12A-123e4567-e89b-12d3-a456-426614174000";
        var response = await _client.GetAsync($"/api/bookings/{bookingId}");

        // Assert: HTTP response status code is NotFound
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Bookings
    [Fact]
    public async Task PostReserveSeat_ReturnsOk()
    {
        // Act
        var flightId = "AA123-2023-10-01T10:00:00Z";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();
        var response = await _client.PostAsync("/api/bookings", JsonContent.Create(new
        {
            FlightId = flightId,
            SeatNumber = seatNumber,
            UserId = userId
        }));

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostReserveSeat_ReturnsNotFound()
    {
        // Act
        var flightId = "AA-2023-10-01T10:00:00Z";
        var seatNumber = "12A";
        var userId = Guid.NewGuid();
        var response = await _client.PostAsync("/api/bookings", JsonContent.Create(new
        {
            FlightId = flightId,
            SeatNumber = seatNumber,
            UserId = userId
        }));

        // Assert: HTTP response status code is NotFound
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBookedStatus_ReturnsOk()
    {
        // Act
        var bookingId = "AA123-2023-10-01T10:00:00Z-12A-123e4567-e89b-12d3-a456-426614174000";
        var response = await _client.GetAsync($"/api/bookings/{bookingId}");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Queue
    [Fact]
    public async Task PostEnqueue_ReturnsOk()
    {
        // Act
        var id = Guid.NewGuid();
        var requestTime = DateTimeOffset.UtcNow;
        var idempotencyKey = Guid.NewGuid().ToString();
        var response = await _client.PostAsync("/api/queue", JsonContent.Create(new
        {
            UserId = id,
            RequestTime = requestTime,
            IdempotencyKey = idempotencyKey
        }));

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetQueueStatus_ReturnsOk()
    {
        // Act
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/queue/{id}");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Sessions
    [Fact]
    public async Task GetSessionStatus_ReturnsOk()
    {
        // Act
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/sessions/{id}");

        // Assert: HTTP response status code is OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}