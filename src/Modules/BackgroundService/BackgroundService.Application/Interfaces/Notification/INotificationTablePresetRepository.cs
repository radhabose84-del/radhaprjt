namespace BackgroundService.Application.Interfaces.Notification
{
    public interface INotificationTablePresetRepository
    {
        Task<string?> GetColumnsJsonByTemplateIdAsync(int templateId, CancellationToken ct);
    }
}
