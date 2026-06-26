using FinanceManagement.Application.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Jobs;

/// <summary>
/// US-GL01-11 — real Hangfire implementation of <see cref="IRecurringTemplateScheduler"/> (overrides the
/// no-op in FinanceManagement.Infrastructure in the Hangfire host). SyncAsync re-evaluates the template and
/// creates/updates the recurring auto-post job ONLY for an Approved + AutoPost + active (not-ended) template;
/// otherwise it removes the job. The cron is derived from the frequency anchored at the start date.
/// </summary>
public sealed class RecurringTemplateScheduler : IRecurringTemplateScheduler
{
    private readonly IRecurringJournalTemplateQueryRepository _templateQuery;
    private readonly ILogger<RecurringTemplateScheduler> _logger;

    public RecurringTemplateScheduler(
        IRecurringJournalTemplateQueryRepository templateQuery,
        ILogger<RecurringTemplateScheduler> logger)
    {
        _templateQuery = templateQuery;
        _logger = logger;
    }

    private static string JobId(int templateId) => $"recurring-template-{templateId}";

    public async Task SyncAsync(int templateId, CancellationToken ct = default)
    {
        var info = await _templateQuery.GetScheduleInfoAsync(templateId);

        var today = DateTime.UtcNow.Date;
        var schedulable = info != null
            && !info.IsDeleted
            && info.IsActive
            && info.AutoPost
            && info.StatusCode == "Approved"
            && (info.EndDate == null || info.EndDate.Value.ToDateTime(TimeOnly.MinValue) >= today);

        if (!schedulable)
        {
            RecurringJob.RemoveIfExists(JobId(templateId));
            _logger.LogInformation("RecurringTemplateScheduler: template {Id} not schedulable — job removed.", templateId);
            return;
        }

        var cron = RecurringTemplateCron.For(info!.FrequencyCode, info.StartDate);
        RecurringJob.AddOrUpdate<RecurringTemplateAutoPostJob>(
            JobId(templateId),
            "journal-jobs-queue",
            job => job.RunAsync(templateId, CancellationToken.None),
            cron);

        _logger.LogInformation("RecurringTemplateScheduler: template {Id} scheduled ({Cron}).", templateId, cron);
    }

    public void Remove(int templateId)
    {
        RecurringJob.RemoveIfExists(JobId(templateId));
        _logger.LogInformation("RecurringTemplateScheduler: template {Id} job removed.", templateId);
    }
}
