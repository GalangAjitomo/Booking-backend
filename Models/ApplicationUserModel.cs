using Microsoft.AspNetCore.Identity;

namespace Booking.Api.Models
{
    public class ApplicationUserModel : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
    }
}
