using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.Forms;
using RHM.Application.Interfaces;
using RHM.Shared.Constants;

namespace RHM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly IFormResponseService _responseService;

    public FormsController(IFormService formService, IFormResponseService responseService)
    {
        _formService = formService;
        _responseService = responseService;
    }

    private string TenantId =>
        User.FindFirstValue(RhmConstants.Claims.TenantId)
            ?? throw new UnauthorizedAccessException("TenantId not found in token.");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var forms = await _formService.GetByTenantAsync(TenantId);
        return Ok(forms);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var forms = (await _formService.GetByTenantAsync(TenantId)).ToList();

        var totalResponses = 0;
        var recentActivity = new List<object>();

        foreach (var form in forms.Where(f => f.IsPublished).OrderByDescending(f => f.PublishedAt).Take(5))
        {
            var responses = (await _responseService.GetByFormAsync(form.Id!)).ToList();
            totalResponses += responses.Count;
            var last = responses.FirstOrDefault();
            if (last != null)
                recentActivity.Add(new { formTitle = form.Title, submittedAt = last.SubmittedAt });
        }

        // Add counts from non-sampled forms
        foreach (var form in forms.Where(f => f.IsPublished).Skip(5))
            totalResponses += (await _responseService.GetByFormAsync(form.Id!)).Count();

        return Ok(new
        {
            formCount = forms.Count,
            publishedCount = forms.Count(f => f.IsPublished),
            totalResponses,
            recentActivity
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var form = await _formService.GetByIdAsync(id);
        if (form is null || form.TenantId != TenantId) return NotFound();
        return Ok(form);
    }

    [HttpPost]
    [Authorize(Roles = "AccountAdmin")]
    public async Task<IActionResult> Create([FromBody] FormSchemaDto dto)
    {
        dto.TenantId = TenantId;
        var created = await _formService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "AccountAdmin")]
    public async Task<IActionResult> Update(string id, [FromBody] FormSchemaDto dto)
    {
        var existing = await _formService.GetByIdAsync(id);
        if (existing is null || existing.TenantId != TenantId) return NotFound();
        var updated = await _formService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "AccountAdmin")]
    public async Task<IActionResult> Publish(string id)
    {
        var existing = await _formService.GetByIdAsync(id);
        if (existing is null || existing.TenantId != TenantId) return NotFound();
        var published = await _formService.PublishAsync(id);
        return Ok(published);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "AccountAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _formService.GetByIdAsync(id);
        if (existing is null || existing.TenantId != TenantId) return NotFound();
        await _formService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/responses")]
    public async Task<IActionResult> GetResponses(string id)
    {
        var form = await _formService.GetByIdAsync(id);
        if (form is null || form.TenantId != TenantId) return NotFound();
        var responses = await _responseService.GetByFormAsync(id);
        return Ok(responses);
    }
}
