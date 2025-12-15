namespace Booking.Api.Models.DTOs
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public bool IsAdmin { get; set; }
    }
}
