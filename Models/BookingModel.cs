using System.ComponentModel.DataAnnotations;

namespace Booking.Api.Models
{
    public class BookingModel
    {
        [Key]
        public Guid BookingId { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUserModel User { get; set; } = default!;

        public Guid RoomId { get; set; }
        public RoomModel Room { get; set; } = default!;

        public DateTime BookingDate { get; set; }
        public string? Purpose { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
