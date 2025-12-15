using Booking.Api.Helpers;
using Booking.Api.Models;
using Booking.Api.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/v1/auth");

        // ================== REGISTER ==================
        group.MapPost("/register", async (
            RegisterDto dto,
            UserManager<ApplicationUserModel> users,
            RoleManager<IdentityRole<Guid>> roles
        ) =>
        {
            if (await users.FindByNameAsync(dto.Username) is not null)
                return Results.Conflict(new { message = "Username already exists" });

            var user = new ApplicationUserModel
            {
                Id = Guid.NewGuid(),
                UserName = dto.Username,
                DisplayName = dto.DisplayName
            };

            var result = await users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Results.BadRequest(result.Errors.Select(e => e.Description));

            // Optional admin role
            if (dto.IsAdmin)
            {
                const string roleName = "Admin";
                if (!await roles.RoleExistsAsync(roleName))
                    await roles.CreateAsync(new IdentityRole<Guid>(roleName));

                await users.AddToRoleAsync(user, roleName);
            }

            return Results.Created(
                $"/api/v1/users/{user.Id}",
                new { user.Id, user.UserName }
            );
        })
        .WithName("Register");

        // ================== LOGIN ==================
        group.MapPost("/login", async (
            LoginDto dto,
            UserManager<ApplicationUserModel> userManager,
            IConfiguration cfg
        ) =>
        {
            var user = await userManager.FindByNameAsync(dto.Username);
            if (user is null)
                return Results.Unauthorized();

            var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);

            // Generate JWT
            var token = JwtTokenHelper.GenerateToken(user, roles, cfg);

            var expiresMinutes = int.Parse(cfg["Jwt:ExpiresMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

            return Results.Ok(new
            {
                accessToken = token,
                tokenType = "Bearer",
                expiresAt,
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.DisplayName,
                    roles
                }
            });
        })
        .WithName("Login");

        return routes;
    }
}