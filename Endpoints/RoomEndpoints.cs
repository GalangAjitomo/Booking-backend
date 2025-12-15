using Booking.Api.Data;
using Booking.Api.Models;
using Booking.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

public static class RoomEndpoints
{
    public static IEndpointRouteBuilder MapRoomEndpoints(this IEndpointRouteBuilder routes)
    {
        // Group-level authorization (official minimal API approach)
        var group = routes
            .MapGroup("/api/v1/rooms")
            .RequireAuthorization();

        // ================== GET ==================
        group.MapGet("/", async (AppDbContext db) =>
        {
            var rooms = await db.Rooms.ToListAsync();
            return Results.Ok(rooms);
        })
        .WithName("GetRooms");

        // ================== POST ==================
        group.MapPost("/", async (
            CreateRoomDto dto,
            AppDbContext db
        ) =>
        {
            var room = new RoomModel
            {
                RoomId = Guid.NewGuid(),
                Code = dto.Code,
                Name = dto.Name,
                Capacity = dto.Capacity,
                Location = dto.Location
            };

            db.Rooms.Add(room);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/rooms/{room.RoomId}",
                room
            );
        })
        .WithName("CreateRoom");

        // ================== PUT ==================
        group.MapPut("/{id:guid}", async (
            Guid id,
            CreateRoomDto dto,
            AppDbContext db
        ) =>
        {
            var room = await db.Rooms.FindAsync(id);
            if (room is null)
                return Results.NotFound();

            room.Code = dto.Code;
            room.Name = dto.Name;
            room.Capacity = dto.Capacity;
            room.Location = dto.Location;

            await db.SaveChangesAsync();
            return Results.Ok(room);
        })
        .WithName("UpdateRoom");

        // ================== DELETE ==================
        group.MapDelete("/{id:guid}", async (
            Guid id,
            AppDbContext db
        ) =>
        {
            var room = await db.Rooms.FindAsync(id);
            if (room is null)
                return Results.NotFound();

            db.Rooms.Remove(room);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteRoom");

        return routes;
    }
}
