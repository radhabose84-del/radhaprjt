using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Jobs;

/// <summary>
/// US-GL01-11 — per-template recurring generation job. Scheduled (by <c>RecurringTemplateScheduler</c>) ONLY for an
/// Approved + AutoPost template, with a cron derived from its frequency/start date. On each fire it generates the
/// template's JV for the current period — NEVER auto-posting it (low-risk → APPROVED, high-risk → DRAFT + approval);
/// posting is always a separate manual/approval step. Idempotent per company+template+period, so a missed/duplicate
/// run is harmless. Company comes from the template (CompanyId) and currency from each line — no session.
/// </summary>
public class RecurringTemplateAutoPostJob
{
    private readonly IRecurringJournalGenerationService _generationService;
    private readonly IRecurringJournalTemplateQueryRepository _templateQuery;
    private readonly ITimeZoneService _timeZoneService;
    private readonly ILogger<RecurringTemplateAutoPostJob> _logger;

    public RecurringTemplateAutoPostJob(
        IRecurringJournalGenerationService generationService,
        IRecurringJournalTemplateQueryRepository templateQuery,
        ITimeZoneService timeZoneService,
        ILogger<RecurringTemplateAutoPostJob> logger)
    {
        _generationService = generationService;
        _templateQuery = templateQuery;
        _timeZoneService = timeZoneService;
        _logger = logger;
    }

    [Queue("journal-jobs-queue")]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task RunAsync(int templateId, CancellationToken cancellationToken)
    {
        var info = await _templateQuery.GetScheduleInfoAsync(templateId);
        if (info == null || info.IsDeleted || !info.IsActive || info.AutoPost == false || info.StatusCode != "Approved")
        {
            _logger.LogInformation("RecurringTemplateAutoPostJob: template {Id} no longer schedulable — skipping.", templateId);
            return;
        }

        var today = DateOnly.FromDateTime(_timeZoneService.GetCurrentTime().DateTime);

        // Generate only — never auto-posts. Low-risk → APPROVED; high-risk → DRAFT + approval.
        // The accounting period is resolved from the date inside the service.
        var journalId = await _generationService.GenerateForTemplateAsync(info.CompanyId, templateId, today, cancellationToken);

        _logger.LogInformation("RecurringTemplateAutoPostJob: template {Id} ({Date}) → journal {JournalId}.",
            templateId, today, journalId);
    }
}
