using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Users;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Identity;
using Hardware.Domain.Entities.Identity;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Identity;

public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : IUserService
{
    public async Task<PagedResult<UserDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default)
    {
        IQueryable<ApplicationUser> query = userManager.Users.Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "username"  => request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "email"     => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "firstname" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname"  => request.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            _           => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            dtos.Add(ToDto(user, roles));
        }

        return PagedResult<UserDto>.Create(dtos, request.Page, request.PageSize, total);
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await FindActiveUserAsync(id);
        var roles = await userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        foreach (var role in dto.Roles)
        {
            if (!RoleConstants.All.Contains(role))
                throw new BusinessException($"'{role}' is not a valid role.");
            if (!await roleManager.RoleExistsAsync(role))
                throw new BusinessException($"Role '{role}' does not exist in the system.");
        }

        if (await userManager.FindByEmailAsync(dto.Email) is not null)
            throw new ConflictException("Email is already registered.");

        if (await userManager.FindByNameAsync(dto.UserName) is not null)
            throw new ConflictException("Username is already taken.");

        var user = new ApplicationUser
        {
            UserName          = dto.UserName,
            Email             = dto.Email,
            EmailConfirmed    = true,
            PhoneNumber       = dto.PhoneNumber,
            PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(dto.PhoneNumber),
            FirstName         = dto.FirstName,
            LastName          = dto.LastName,
            CreatedAt         = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BusinessException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRolesAsync(user, dto.Roles);
        if (!roleResult.Succeeded)
            throw new BusinessException(string.Join("; ", roleResult.Errors.Select(e => e.Description)));

        return ToDto(user, dto.Roles);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await FindActiveUserAsync(id);

        user.FirstName   = dto.FirstName;
        user.LastName    = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(dto.PhoneNumber);
        user.UpdatedAt   = DateTime.UtcNow;

        if (!dto.IsActive && !user.IsDeleted)
        {
            user.LockoutEnabled  = true;
            user.LockoutEnd      = DateTimeOffset.MaxValue;
        }
        else if (dto.IsActive && user.LockoutEnd == DateTimeOffset.MaxValue)
        {
            user.LockoutEnd = null;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BusinessException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roles = await userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await FindActiveUserAsync(id);

        user.IsDeleted  = true;
        user.UpdatedAt  = DateTime.UtcNow;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BusinessException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<UserDto> AssignRolesAsync(Guid id, AssignRolesDto dto, CancellationToken ct = default)
    {
        var user = await FindActiveUserAsync(id);

        foreach (var role in dto.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                throw new BusinessException($"Role '{role}' does not exist in the system.");
        }

        var currentRoles = await userManager.GetRolesAsync(user);

        if (currentRoles.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                throw new BusinessException(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        }

        if (dto.Roles.Any())
        {
            var addResult = await userManager.AddToRolesAsync(user, dto.Roles);
            if (!addResult.Succeeded)
                throw new BusinessException(string.Join("; ", addResult.Errors.Select(e => e.Description)));
        }

        return ToDto(user, dto.Roles);
    }

    private async Task<ApplicationUser> FindActiveUserAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("User", id);

        if (user.IsDeleted)
            throw new NotFoundException("User", id);

        return user;
    }

    private static UserDto ToDto(ApplicationUser user, IEnumerable<string> roles) =>
        new(user.Id,
            user.UserName!,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.LockoutEnd is null || user.LockoutEnd < DateTimeOffset.UtcNow,
            user.CreatedAt,
            roles.ToList());
}
