using System.ComponentModel.DataAnnotations;

namespace Booking.Api.Models
{
    public class RoomModel
    {
        [Key]
        public Guid RoomId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Capacity { get; set; }
        public string? Location { get; set; }
    }
}
