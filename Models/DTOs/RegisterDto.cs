namespace Booking.Api.Models.DTOs
{
    public record RegisterDto(string Username, string Password, string DisplayName, bool IsAdmin = false);
}
