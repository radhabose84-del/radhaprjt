// BackgroundService.Application/Interfaces/Notification/INotificationResolverHandler.cs
using System;

namespace BackgroundService.Application.Interfaces.Notification
{
    public interface INotificationResolverHandler
    {
        Task<List<string>> ResolveNotificationChannelsAsync(
            int unitId, string module, int eventTypeId,
            string email, string ccMail, string mobile);

        Task<(List<string> To, List<string> Cc, List<string> Bcc,
           List<string> Sms, List<int> InApp,
           string Subject, string Header, string Body, string Footer,
           string Lang, int? EventTypeId, int? EventRuleId, int? ChannelId,
           int TemplateId, bool IsTable,string? ApiToken)>

        ResolveNotificationTemplatesAsync(
            int unitId, string module, int eventTypeId,
            string email, string ccMail, string mobile,
            string p1, string p2, DateTimeOffset p3,
            string? p4, string? p5, string? p6, string? p7, string? p8, string? p9, string? p10,
            IReadOnlyCollection<(string FileName, string ContentType, byte[] Data)>? attachments = null);
    }
}
