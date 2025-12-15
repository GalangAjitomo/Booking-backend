using Booking.Api.Models;
using Booking.Api.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/api/v1/users")
                      .RequireAuthorization();

        // =========================
        // ADMIN - LIST USERS
        // =========================
        g.MapGet("/", async (UserManager<ApplicationUserModel> userMgr) =>
        {
            return userMgr.Users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                DisplayName = u.DisplayName!
            });
        })
        .RequireAuthorization(r => r.RequireRole("Admin"));

        // =========================
        // GET USER BY ID (ADMIN / OWNER)
        // =========================
        g.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            UserManager<ApplicationUserModel> userMgr
        ) =>
        {
            if (!CanAccessUser(ctx, id))
                return Results.Forbid();

            var user = await userMgr.FindByIdAsync(id.ToString());
            if (user is null) return Results.NotFound();

            return Results.Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                DisplayName = user.DisplayName!
            });
        });

        // =========================
        // ADMIN - CREATE USER
        // =========================
        g.MapPost("/", async (
            CreateUserDto dto,
            UserManager<ApplicationUserModel> userMgr,
            RoleManager<IdentityRole<Guid>> roleMgr
        ) =>
        {
            if (await userMgr.FindByNameAsync(dto.UserName) is not null)
                return Results.Conflict("Username already exists");

            var user = new ApplicationUserModel
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName,
                DisplayName = dto.DisplayName
            };

            var res = await userMgr.CreateAsync(user, dto.Password);
            if (!res.Succeeded)
                return Results.BadRequest(res.Errors);

            if (dto.IsAdmin)
            {
                if (!await roleMgr.RoleExistsAsync("Admin"))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>("Admin"));

                await userMgr.AddToRoleAsync(user, "Admin");
            }

            return Results.Created(
                $"/api/v1/users/{user.Id}",
                new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    DisplayName = user.DisplayName!
                }
            );
        })
        .RequireAuthorization(r => r.RequireRole("Admin"));

        // =========================
        // UPDATE USER (ADMIN / OWNER)
        // =========================
        g.MapPut("/{id:guid}", async (
            Guid id,
            UpdateUserDto dto,
            HttpContext ctx,
            UserManager<ApplicationUserModel> userMgr
        ) =>
        {
            if (!CanAccessUser(ctx, id))
                return Results.Forbid();

            var user = await userMgr.FindByIdAsync(id.ToString());
            if (user is null) return Results.NotFound();

            user.DisplayName = dto.DisplayName;
            await userMgr.UpdateAsync(user);

            return Results.Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                DisplayName = user.DisplayName!
            });
        });

        // =========================
        // DELETE USER (ADMIN / OWNER)
        // =========================
        g.MapDelete("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            UserManager<ApplicationUserModel> userMgr
        ) =>
        {
            if (!CanAccessUser(ctx, id))
                return Results.Forbid();

            var user = await userMgr.FindByIdAsync(id.ToString());
            if (user is null) return Results.NotFound();

            await userMgr.DeleteAsync(user);
            return Results.NoContent();
        });

        return routes;
    }

    // =========================
    // ACCESS CHECK
    // =========================
    private static bool CanAccessUser(HttpContext ctx, Guid targetUserId)
    {
        if (ctx.User.IsInRole("Admin"))
            return true;

        var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId == targetUserId.ToString();
    }
}