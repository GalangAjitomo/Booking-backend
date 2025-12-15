namespace Booking.Api.Models.DTOs
{
    public class BookingListDto
    {
        public Guid BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Purpose { get; set; }

        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = default!;

        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
    }
}
