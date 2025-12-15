using Microsoft.AspNetCore.Identity;
using Booking.Api.Models;

namespace Booking.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUserModel>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var adminRole = "Admin";
        if (!await roleMgr.RoleExistsAsync(adminRole))
            await roleMgr.CreateAsync(new IdentityRole<Guid>(adminRole));

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var adminUsername = config["SeedUsers:Admin:Username"] ?? "admin";
        var adminPassword = config["SeedUsers:Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Admin seed password is not configured. Please set it via user-secrets or environment variables."
            );
        }

        var adminUser = await userMgr.FindByNameAsync(adminUsername);
        if (adminUser is null)
        {
            adminUser = new ApplicationUserModel
            {
                Id = Guid.NewGuid(),
                UserName = adminUsername,
                DisplayName = "Administrator",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var res = await userMgr.CreateAsync(adminUser, adminPassword);
            if (res.Succeeded)
            {
                await userMgr.AddToRoleAsync(adminUser, adminRole);
            }
        }

        // ================== SEED ROOMS ==================
        if (!db.Rooms.Any())
        {
            db.Rooms.AddRange(
                new RoomModel
                {
                    RoomId = Guid.NewGuid(),
                    Code = "KLB-MTG-01",
                    Name = "Kalbe Executive Meeting Room",
                    Capacity = 12,
                    Location = "Head Office - Floor 3"
                },
                new RoomModel
                {
                    RoomId = Guid.NewGuid(),
                    Code = "KLB-MTG-02",
                    Name = "Kalbe Project War Room",
                    Capacity = 8,
                    Location = "Head Office - Floor 2"
                },
                new RoomModel
                {
                    RoomId = Guid.NewGuid(),
                    Code = "KLB-TRN-01",
                    Name = "Kalbe Training Room",
                    Capacity = 30,
                    Location = "Learning Center - Floor 1"
                },
                new RoomModel
                {
                    RoomId = Guid.NewGuid(),
                    Code = "KLB-BRD-01",
                    Name = "Kalbe Board Room",
                    Capacity = 16,
                    Location = "Head Office - Floor 5"
                },
                new RoomModel
                {
                    RoomId = Guid.NewGuid(),
                    Code = "KLB-INT-01",
                    Name = "Kalbe Interview Room",
                    Capacity = 4,
                    Location = "HR Area - Floor 2"
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
