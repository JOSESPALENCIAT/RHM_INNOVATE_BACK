using Microsoft.AspNetCore.Mvc;
using RHM.Application.DTOs.Forms;
using RHM.Application.Interfaces;

namespace RHM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly IFormResponseService _responseService;
    private readonly IWebhookService _webhook;

    public PublicController(
        IFormService formService,
        IFormResponseService responseService,
        IWebhookService webhook)
    {
        _formService = formService;
        _responseService = responseService;
        _webhook = webhook;
    }

    [HttpGet("forms/{publicUrl}")]
    public async Task<IActionResult> GetForm(string publicUrl)
    {
        var form = await _formService.GetByPublicUrlAsync(publicUrl);
        return form is null ? NotFound() : Ok(form);
    }

    [HttpPost("forms/{formId}/submit")]
    public async Task<IActionResult> Submit(string formId, [FromBody] SubmitFormDto dto)
    {
        var form = await _formService.GetByIdAsync(formId);
        if (form is null || !form.IsPublished) return NotFound();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _responseService.SubmitAsync(formId, form.TenantId, dto, ip);

        // Fire-and-forget: send to n8n for AI processing (does not block the HTTP response)
        _ = Task.Run(() => _webhook.TriggerAsync(new
        {
            formId,
            formTitle = form.Title,
            tenantId = form.TenantId,
            responseId = response.Id,
            data = response.Data,
            latitude = response.Latitude,
            longitude = response.Longitude,
            submittedAt = response.SubmittedAt
        }));

        return Ok(response);
    }
}
