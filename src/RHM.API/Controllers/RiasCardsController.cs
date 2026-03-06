using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.Rias;
using RHM.Application.Interfaces;
using RHM.Shared.Constants;

namespace RHM.API.Controllers;

[ApiController]
[Route("api/rias-cards")]
[Authorize]
public class RiasCardsController : ControllerBase
{
    private readonly IRiasCardService _riasService;

    public RiasCardsController(IRiasCardService riasService) => _riasService = riasService;

    private string UserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? string.Empty;

    /// <summary>Any authenticated user: get the global RIAS config (null = use static defaults).</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await _riasService.GetAsync();
        return Ok(config);
    }

    /// <summary>SuperAdmin: save (upsert) the global RIAS config for all tenants.</summary>
    [HttpPut]
    [Authorize(Roles = RhmConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> Save([FromBody] TenantRiasConfigDto dto)
    {
        var saved = await _riasService.SaveAsync(UserId, dto);
        return Ok(saved);
    }

    /// <summary>SuperAdmin: reset global config to static defaults.</summary>
    [HttpDelete]
    [Authorize(Roles = RhmConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> Reset()
    {
        await _riasService.DeleteAsync();
        return NoContent();
    }
}
