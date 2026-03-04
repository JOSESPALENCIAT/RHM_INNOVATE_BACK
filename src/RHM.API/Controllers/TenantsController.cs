using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.Interfaces;
using RHM.Infrastructure.Persistence;
using RHM.Shared.Constants;
using MongoDB.Driver;

namespace RHM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RhmConstants.Roles.SuperAdmin)]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly MongoDbContext _mongo;

    public TenantsController(ITenantService tenantService, MongoDbContext mongo)
    {
        _tenantService = tenantService;
        _mongo = mongo;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var tenants = await _tenantService.GetAllAsync();
        var activeTenants = tenants.Count(t => t.IsActive);
        var publishedForms = await _mongo.FormSchemas.CountDocumentsAsync(f => f.IsPublished);
        var totalResponses = await _mongo.FormResponses.CountDocumentsAsync(FilterDefinition<RHM.Infrastructure.Documents.FormResponse>.Empty);

        return Ok(new
        {
            activeTenants,
            totalTenants = tenants.Count(),
            publishedForms,
            totalResponses
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _tenantService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] bool isActive)
    {
        await _tenantService.SetActiveAsync(id, isActive);
        return NoContent();
    }
}
