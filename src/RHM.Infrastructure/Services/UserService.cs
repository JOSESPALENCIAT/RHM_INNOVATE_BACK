using RHM.Application.DTOs.Users;
using RHM.Application.Interfaces;
using RHM.Domain.Entities;
using RHM.Domain.Enums;
using RHM.Domain.Interfaces;
using RHM.Shared.Constants;

namespace RHM.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<IEnumerable<UserDto>> GetByTenantAsync(Guid tenantId)
    {
        var users = await _userRepo.GetByTenantAsync(tenantId);
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(Guid tenantId, CreateUserDto dto)
    {
        var count = await _userRepo.CountByTenantAsync(tenantId);
        if (count >= RhmConstants.MaxUsersPerTenant)
            throw new InvalidOperationException($"Límite de {RhmConstants.MaxUsersPerTenant} usuarios por cuenta alcanzado.");

        if (!Enum.TryParse<UserRole>(dto.Role, out var role) || role == UserRole.SuperAdmin)
            role = UserRole.Operator;

        var user = await _userRepo.CreateAsync(new User
        {
            TenantId = tenantId,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            IsActive = true
        });

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;
        await _userRepo.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task DeleteAsync(Guid id) => await _userRepo.DeleteAsync(id);

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        TenantId = u.TenantId
    };
}
