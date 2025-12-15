using Booking.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Booking.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUserModel, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RoomModel> Rooms { get; set; } = null!;
    public DbSet<BookingModel> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BookingModel>()
            .HasIndex(b => new { b.RoomId, b.BookingDate })
            .IsUnique();
    }
}