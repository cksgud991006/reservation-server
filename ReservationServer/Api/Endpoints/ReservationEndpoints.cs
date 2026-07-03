using ReservationServer.Application.Services;
using ReservationServer.Infrastructure.Redis;
using ReservationServer.Api.Dto;
using ReservationServer.Application.Repositories;
using ReservationServer.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ReservationServer.Api.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this WebApplication app)
    {
        // Flights
        app.MapGet("/api/flights", GetFlightInstances);
        app.MapGet("/api/flights/{flightNumber}/{departureTime}", GetFlightInstance);
        app.MapGet("/api/flights/{flightNumber}/seats", GetSeatLayout);
        app.MapGet("/api/flights/{flightId}/seats/count", GetFlightSeatCount);
        app.MapGet("/api/flights/{flightId}/bookings", GetFlightBookings);

        // Bookings
        app.MapPost("/api/bookings", ReserveSeat);
        app.MapGet("/api/bookings/{bookingId}", GetBookedStatus);

        // Queue
        app.MapPost("/api/queue", Enqueue);
        app.MapGet("/api/queue/{id}", GetQueueStatus);

        // Sessions
        app.MapGet("/api/sessions/{id}", GetSessionStatus);
    }

    private static async Task<IResult> GetFlightInstances(
        ISeatInventoryRepository service)
    {
        var response = await service.GetFlightInstances();

        return Results.Ok(
            response
        );
    }

    private static async Task<IResult> GetFlightInstance(
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

    private static async Task<IResult> GetSeatLayout(
        string flightNumber,
        ISeatInventoryRepository service)
    {
        var response = await service.GetSeatLayout(flightNumber);

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

    private static async Task<IResult> GetFlightBookings(
        string flightId,
        ISeatInventoryRepository service)
    {
        var response = await service.GetFlightBookings(flightId);

        return Results.Ok(
            response
        );
    }



    private static async Task<IResult> ReserveSeat(
        ReservationBookRequest request,
        ISeatInventoryService service)
    {
        var result = await service.ReserveSeatAsync(request.FlightId, request.SeatNumber, request.UserId);
        // TODO: process return to proper result

        return result switch
        {
            { Success: true } =>
                Results.Created(
                    $"/api/bookings/{result.BookingId}",
                    new ReservationBookResponse(
                        result.FlightId,
                        result.SeatNumber,
                        result.UserId,
                        result.BookingId,
                        result.Details
                    )
                ),

            { Success: false } =>
                Results.NotFound(
                    new ReservationBookFailureResponse(
                        result.Details
                    )
                ),
            _ =>
                Results.StatusCode(500)
        };
    }

    private static async Task<IResult> GetBookedStatus(
        string bookingId,
        ISeatInventoryRepository service)
    {

        var result = await service.GetBookedStatusAsync(bookingId);

        if (result == null)
        {
            return Results.NotFound();
        }

        return Results.Ok();
    }

    private static async Task<IResult> Enqueue(
        ReservationWaitRequest request,
        IQueueService service)
    {
        await service.EnqueueAsync(request.UserId, request.RequestTime);
        
        return Results.Ok(
            new EnqueueResponse(true)
        );
    }

    private static async Task<IResult> GetQueueStatus(
        Guid id,
        IQueueService service)
    {
        // polling queue status from redis
        var queueResponse = await service.GetPositionInQueueAsync(id);

        return queueResponse switch
        {
            { IsInQueue: true } =>
                Results.Ok(
                    new ReservationWaitResponse(
                        id,
                        queueResponse.Position
                    )
                ),
            { IsInQueue: false } =>
                Results.Ok(
                    new ReservationWaitResponse(
                        id,
                        -1
                    )
                ),
            _ =>
                Results.StatusCode(500)
        };
    }

    private static async Task<IResult> GetSessionStatus(
        Guid id,
        IRedisSession service)
    {
        // polling session status from redis
        var sessionStatus = await service.GetSessionStatusAsync(id);

        return sessionStatus switch
        {
            { IsActive: true } =>
                Results.Ok(
                    new ReservationSessionResponse(
                        id,
                        sessionStatus.TimeExpiry.ToUnixTimeSeconds()
                    )
                ),
            { IsActive: false } =>
                Results.Ok(
                    new ReservationSessionResponse(
                        id,
                        -1
                    )
                ),
            _ =>
                Results.StatusCode(500)
        };
    }
}
