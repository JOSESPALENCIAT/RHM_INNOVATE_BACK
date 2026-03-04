namespace RHM.Application.Interfaces;

public interface IWebhookService
{
    /// <summary>
    /// Fires a webhook to n8n (or any configured URL) with the form response payload.
    /// Fire-and-forget: errors are logged but do not affect the response to the user.
    /// </summary>
    Task TriggerAsync(object payload);
}
