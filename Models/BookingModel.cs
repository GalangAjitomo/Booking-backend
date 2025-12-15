using System.ComponentModel.DataAnnotations;

namespace Booking.Api.Models
{
    public class BookingModel
    {
        [Key]
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Purpose { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
