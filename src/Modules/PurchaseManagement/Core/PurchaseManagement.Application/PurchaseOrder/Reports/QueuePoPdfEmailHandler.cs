 using Contracts.Events.Notifications;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.Reports;

public sealed class QueuePoPdfEmailHandler(
    ISsrsClient ssrs,
    IFileStorage storage,
    IPublishEndpoint bus,
    IConfiguration cfg,
    ILogger<QueuePoPdfEmailHandler> log
) : IRequestHandler<QueuePoPdfEmailCommand, Guid>
{
    public async Task<Guid> Handle(QueuePoPdfEmailCommand cmd, CancellationToken ct)
    {
        var corr = Guid.NewGuid();

        // 1) Render SSRS once
        var reportPath = cfg["Ssrs:PoReportPath"] ?? "/bsoft/PO";
        //var pdf = await ssrs.RenderPdfAsync(reportPath, new Dictionary<string, string?>
        var pdf = await ssrs.RenderPdfAsync(
            reportPath,
            new Dictionary<string, string?>
            {
                ["unitid"] = cmd.UnitId.ToString(),
                ["PoId"]   = cmd.PoId.ToString()
            },
            ct);

        // 2) Upload once
        var fileName = $"PO-{cmd.PoId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var blobKey = $"notifications/{corr}/{fileName}";
        var blobUrl = await storage.PutAsync("reports", blobKey, pdf, "application/pdf", ct);

        // 3) Choose recipients from PartyContacts
        var recipients = (cmd.PartyContacts ?? new())
            .Where(p => !string.IsNullOrWhiteSpace(p.Email))
            .Select(p => new { Email = p.Email.Trim(), Name = p.Name?.Trim() ?? "" })
            .GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (recipients.Count == 0)
        {
            log.LogWarning("No valid recipient emails in PartyContacts for PO {PoId}", cmd.PoId);
            return corr;
        }

        var eventTypeId = int.TryParse(cfg["Notifications:POApprovedEventTypeId"], out var e) ? e : 14;


        // 4) Publish one notification per recipient
        foreach (var rcpt in recipients)
        {
            var toEmail = rcpt.Email.Trim();
            var toName = string.IsNullOrWhiteSpace(rcpt.Name) ? "" : rcpt.Name.Trim();

            var evt = new NotificationCreatedEvent
            {
                CorrelationId = corr,
                CreatedByName = "System",
                UnitId = cmd.UnitId,
                ModuleName = "POLocal",
                EventTypeId = eventTypeId,
                Email = toEmail,
                ccMail = "",   // or build CC from the rest of contacts if you prefer single email
                Mobile = "",
                param1 = string.IsNullOrEmpty(toName) ? "" : toName, // name
                param2 = cmd.PoId.ToString(),
                param3 = DateTimeOffset.UtcNow,
                param4 = "",
                param10 = cmd.RowsJson,
                ModuleTransactionId = cmd.PoId,
                ModuleTypeName = "Purchase Order",
                Attachments =
                {
                    new NotificationCreatedEvent.NotificationAttachment
                    {
                        FileName    = fileName,
                        ContentType = "application/pdf",
                        BlobUrl     = blobUrl,
                        IsPrivate   = true
                    }
                }
            };

            await bus.Publish(evt, ct);
            log.LogInformation("PO email queued | CorrId={Corr} To={To} File={File}", corr, toEmail, blobUrl);
        }

        return corr;
    }
}
