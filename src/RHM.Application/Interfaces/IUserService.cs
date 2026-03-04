using RHM.Application.DTOs.Users;

namespace RHM.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetByTenantAsync(Guid tenantId);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(Guid tenantId, CreateUserDto dto);
    Task<UserDto> UpdateAsync(Guid id, UserDto dto);
    Task DeleteAsync(Guid id);
}
