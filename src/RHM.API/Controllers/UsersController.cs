using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.Users;
using RHM.Application.Interfaces;
using RHM.Shared.Constants;
using System.Security.Claims;

namespace RHM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    [Authorize(Roles = $"{RhmConstants.Roles.SuperAdmin},{RhmConstants.Roles.AccountAdmin}")]
    public async Task<IActionResult> GetByTenant()
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();
        return Ok(await _userService.GetByTenantAsync(tenantId.Value));
    }

    [HttpPost]
    [Authorize(Roles = $"{RhmConstants.Roles.SuperAdmin},{RhmConstants.Roles.AccountAdmin}")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();
        try
        {
            var user = await _userService.CreateAsync(tenantId.Value, dto);
            return CreatedAtAction(nameof(GetByTenant), new { }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{RhmConstants.Roles.SuperAdmin},{RhmConstants.Roles.AccountAdmin}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }

    private Guid? GetTenantId()
    {
        var claim = User.FindFirst(RhmConstants.Claims.TenantId)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
