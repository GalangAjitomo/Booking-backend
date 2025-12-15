using Booking.Api.Data;
using Booking.Api.Models;
using Booking.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        // Group-level authorization (official minimal API way)
        var group = routes
            .MapGroup("/api/v1/bookings")
            .RequireAuthorization();

        // ================== GET ==================
        group.MapGet("/", async (
            AppDbContext db,
            DateTime? date,
            Guid? roomId
        ) =>
        {
            var query = db.Bookings
                .AsNoTracking()
                .Include(b => b.Room)
                .Include(b => b.User)
                .AsQueryable();

            if (date.HasValue)
                query = query.Where(b => b.BookingDate == date.Value.Date);

            if (roomId.HasValue)
                query = query.Where(b => b.RoomId == roomId.Value);

            var result = await query
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingListDto
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    Purpose = b.Purpose,

                    RoomId = b.RoomId,
                    RoomName = b.Room.Name,

                    UserId = b.UserId,
                    UserName = b.User.UserName!
                })
                .ToListAsync();

            return Results.Ok(result);
        })
        .WithName("GetBookings");


        // ================== POST ==================
        group.MapPost("/", async (
            CreateBookingDto dto,
            AppDbContext db,
            HttpContext http
        ) =>
        {
            // Official way: read user id from claims
            var userIdClaim =
                http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? http.User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (userIdClaim is null)
                return Results.Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var bookingDate = dto.BookingDate.Date;

            // conflict check
            var exists = await db.Bookings.AnyAsync(b =>
                b.RoomId == dto.RoomId &&
                b.BookingDate == bookingDate
            );

            if (exists)
                return Results.Conflict(new { message = "Room already booked on this date" });

            var booking = new BookingModel
            {
                BookingId = Guid.NewGuid(),
                RoomId = dto.RoomId,
                UserId = userId,
                BookingDate = bookingDate,
                Purpose = dto.Purpose
            };

            db.Bookings.Add(booking);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/bookings/{booking.BookingId}",
                booking
            );
        })
        .WithName("CreateBooking");

        // ================== DELETE ==================
        group.MapDelete("/{id:guid}", async (
            Guid id,
            AppDbContext db,
            HttpContext http
        ) =>
        {
            var booking = await db.Bookings.FindAsync(id);
            if (booking is null)
                return Results.NotFound();

            var userIdClaim =
                http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? http.User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (userIdClaim is null)
                return Results.Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // Owner OR Admin can delete
            var isAdmin = http.User.IsInRole("Admin");
            if (booking.UserId != userId && !isAdmin)
                return Results.Forbid();

            db.Bookings.Remove(booking);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteBooking");

        return routes;
    }
}