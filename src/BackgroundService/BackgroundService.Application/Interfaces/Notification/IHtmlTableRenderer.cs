using BackgroundService.Application.Notification.Common.Models;
namespace BackgroundService.Application.Interfaces.Notification
{
    public interface IHtmlTableRenderer
    {
        Task<string> RenderAsync(HtmlTableSpec spec, CancellationToken ct = default);
        Task<string> RenderFromTemplateAsync(int templateId, string rowsJson, CancellationToken ct = default);
    }
}
