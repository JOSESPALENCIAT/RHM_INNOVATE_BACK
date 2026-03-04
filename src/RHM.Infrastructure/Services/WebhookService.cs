using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RHM.Application.Interfaces;

namespace RHM.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<WebhookService> _logger;
    private readonly string? _webhookUrl;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public WebhookService(
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<WebhookService> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
        _webhookUrl = configuration["N8n:WebhookUrl"];
    }

    public async Task TriggerAsync(object payload)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            _logger.LogDebug("N8n webhook URL not configured. Skipping.");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOpts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpFactory.CreateClient("n8n");
            var response = await client.PostAsync(_webhookUrl, content);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("N8n webhook returned {StatusCode}", response.StatusCode);
            else
                _logger.LogInformation("N8n webhook triggered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering n8n webhook.");
        }
    }
}
