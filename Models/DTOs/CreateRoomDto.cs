namespace Booking.Api.Models.DTOs
{
    public record CreateRoomDto(string Code, string Name, int Capacity, string? Location);
}
