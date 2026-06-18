using TicketServer.Application.Services;
using TicketServer.Infrastructure.Redis;
using TicketServer.Api.Dto;

namespace TicketServer.Endpoints;

public static class TicketEndpoint
{
    public static void MapTicketEndPoints(this WebApplication app)
    {
        // GET 
        app.MapGet("/queue/status/{id}", GetQueueStatus);
        app.MapGet("/active/status/{id}", GetActiveStatus);
        app.MapGet("/api/booked/{bookingId}", GetBookedStatus);
        
        // POST
        app.MapPost("/queue", Enqueue); 
        app.MapPost("/seat", ReserveSeat);

    }

    private static async Task<IResult> GetQueueStatus(
        Guid id,
        IQueueingService service)
    {
        // polling queue status from redis
        var queueResponse = await service.GetPositionInQueueAsync(id);

        return queueResponse switch
        {
            { IsInQueue: true } =>
                Results.Ok(
                    new TicketWaitResponse(
                        id,
                        queueResponse.Position
                    )
                ),
            { IsInQueue: false } =>
                Results.Ok(
                    new TicketWaitResponse(
                        id,
                        -1
                    )
                ),
            _ =>
                Results.StatusCode(500)
        };
    }

    private static async Task<IResult> GetActiveStatus(
        Guid id,
        IRedisSession service)
    {
        // polling active status from redis
        var sessionStatus = await service.GetSessionStatusAsync(id);

        return sessionStatus switch
        {
            { IsActive: true } =>
                Results.Ok(
                    new TicketSessionResponse(
                        id,
                        sessionStatus.TimeExpiry.ToUnixTimeSeconds()
                    )
                ),
            { IsActive: false } =>
                Results.Ok(
                    new TicketSessionResponse(
                        id,
                        -1
                    )
                ),
            _ =>
                Results.StatusCode(500)
        };
    }

    private static async Task<IResult> Enqueue(
        TicketWaitRequest request,
        IQueueingService service)
    {
        await service.EnqueueAsync(request.UserId, request.RequestTime);
        return Results.Ok(
            new PostResponse(true)
        );
    }

    private static async Task<IResult> ReserveSeat(
        TicketBookRequest request,
        ISeatInventoryService service)
    {
        var result = await service.ReserveSeatAsync(request.FlightId, request.SeatNumber, request.UserId);
        // TODO: process return to proper result
        
        return result switch
        {
            { Success: true } =>
                Results.Created(
                    $"/api/booked/{result.BookingId}",
                    new TicketIssueResponse(
                        result.Success,
                        result.FlightId,
                        result.Date,
                        result.SeatNumber,
                        result.UserId,
                        result.BookingId,
                        result.Details
                    )
                ),

            { Success: false } =>
                Results.Conflict(
                    new TicketIssueFailure(
                        result.Details,
                        result.Success
                    )
                ),
            _ =>
                Results.StatusCode(500)
        };
    }

    private static async Task<IResult> GetBookedStatus(
        string bookingId,
        ISeatInventoryService service)
    {
        // TODO: build api
        //var result = await service.GetBookedSeatAsync(bookingId);
        
        return Results.Ok();
    }
}