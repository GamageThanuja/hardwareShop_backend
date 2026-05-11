using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Users;

namespace Hardware.Application.Services.Identity;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> AssignRolesAsync(Guid id, AssignRolesDto dto, CancellationToken ct = default);
}
