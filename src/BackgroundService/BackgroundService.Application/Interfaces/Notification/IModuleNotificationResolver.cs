namespace BackgroundService.Application.Interfaces.Notification
{
    public interface IModuleNotificationResolver
    {
        bool CanHandle(string module, int eventTypeId);
        Task<ModuleResolution> ResolveAsync(
            int unitId, string module, int eventTypeId,
            string? p1, string? p2, DateTimeOffset? p3,
            string? p4, string? p5, string? p6, string? p7, string? p8, string? p9, string? p10,
            CancellationToken ct = default);
    }

    public sealed record ModuleResolution(
        List<string> To, List<string> Cc, List<string> Bcc,
        List<string> Sms, List<int> InAppUserIds,
        string SubjectTemplate, string HeaderTemplate, string BodyTemplate, string FooterTemplate,
        string LangCode, int? EventRuleId, int? ChannelId,
        Dictionary<string, string> ExtraTokens);
}