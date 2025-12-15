namespace Booking.Api.Models.DTOs
{
    public record CreateBookingDto(Guid RoomId, DateTime BookingDate, string? Purpose);
}
