using TicketServer.Application.Services;
using TicketServer.Infrastructure.Redis;
using TicketServer.Api.Dto;
using TicketServer.Application.Repositories;

namespace TicketServer.Api.Endpoints;

public static class TicketEndpoint
{
    public static void MapTicketEndPoints(this WebApplication app)
    {
        // GET 
        app.MapGet("/queue/status/{id}", GetQueueStatus);
        app.MapGet("/active/status/{id}", GetActiveStatus);
        app.MapGet("/api/booked/{bookingId}", GetBookedStatus);
        app.MapGet("/api/flightId/{flightNumber}/{departureTime}", GetFlightId);
        app.MapGet("/api/flightInstances", GetFlightInstances);
        app.MapGet("/api/flightSeatCount/{flightId}", GetFlightSeatCount);
        app.MapGet("/api/seatLayout/{flightNumber}", GetSeatLayout);
        app.MapGet("/api/flightBooking/{flightId}", GetFlightBookings);
        app.MapGet("/api/flightBooking/{flightId}/{seatNumber}", GetFlightBooking);
        
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

    private static async Task<IResult> GetBookedStatus(
        string bookingId,
        ISeatInventoryService service)
    {
        // TODO: build api
        //var result = await service.GetBookedSeatAsync(bookingId);
        
        return Results.Ok();
    }

    private static async Task<IResult> GetFlightId(
        string flightNumber,
        string departureTime,
        ISeatInventoryRepository service)
    {
        var time = Format.FormatDate(departureTime);
        var response = await service.GetFlightInstance(flightNumber, time);

        if (response == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(
            response
        );
    }
    
    private static async Task<IResult> GetFlightInstances(
        ISeatInventoryRepository service)
    {
        var response = await service.GetFlightInstances();

        if (response == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(
            response
        );
    }
    
    private static async Task<IResult> GetFlightSeatCount(
        string flightId,
        ISeatInventoryRepository service)
    {
        var response = await service.GetFlightSeatCount(flightId);

        if (response == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(
            response
        );
    }

    private static async Task<IResult> GetSeatLayout(
        string flightNumber,
        ISeatInventoryRepository service)
    {
        var response = await service.GetSeatLayout(flightNumber);

        if (response == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(
            response
        );
    }

    private static async Task<IResult> GetFlightBookings(
        string flightId,
        ISeatInventoryRepository service)
    {
        var response = await service.GetFlightBookings(flightId);

        if (response == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(
            response
        );
    }

    private static async Task<IResult> GetFlightBooking(
        string flightId,
        string seatNumber,
        ISeatInventoryRepository service)
    {
        var response = await service.GetFlightBooking(flightId, seatNumber);

        if (response == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(
            response
        );
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
}
