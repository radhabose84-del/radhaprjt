using System.Globalization;
using System.Text;
using Contracts.Events.Notifications;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Application.JournalMaster.Dto;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundService.Infrastructure.Jobs
{
    /// <summary>
    /// US-GL03-04 / AC#4 — every Monday at 08:00 UTC, scans the last N days of postings
    /// (default 7) for every company, groups the backdated journals, and emails a digest to
    /// CFO + FC role recipients per the BackdateDigest options. Skip-safe: empty buckets are
    /// not emailed, and if no recipients are resolvable the company is logged + skipped.
    /// Runs in the 'finance-jobs' Hangfire queue with 3 retries on failure.
    /// </summary>
    public class WeeklyBackdatedJournalDigestJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklyBackdatedJournalDigestJob> _logger;

        public WeeklyBackdatedJournalDigestJob(
            IServiceScopeFactory scopeFactory,
            ILogger<WeeklyBackdatedJournalDigestJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        [Queue("finance-jobs")]
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var options       = scope.ServiceProvider.GetRequiredService<IOptions<BackdateDigestOptions>>().Value;
            var companyLookup = scope.ServiceProvider.GetRequiredService<ICompanyLookup>();
            var roleLookup    = scope.ServiceProvider.GetRequiredService<IRoleUserLookup>();
            var queryRepo     = scope.ServiceProvider.GetRequiredService<IJournalQueryRepository>();
            var mediator      = scope.ServiceProvider.GetRequiredService<IMediator>();

            var windowDays = options.WindowDays > 0 ? options.WindowDays : 7;
            var windowEnd   = DateTimeOffset.UtcNow;
            var windowStart = windowEnd.AddDays(-windowDays);

            if (options.CfoRoleId <= 0 && options.FcRoleId <= 0)
            {
                _logger.LogWarning(
                    "WeeklyBackdatedJournalDigestJob: BackdateDigest:CfoRoleId / FcRoleId are not configured — skipping run.");
                return;
            }

            // Resolve recipients once per run. Per-company filtering by access is intentionally not
            // applied here — the CFO/FC roles are global, and the digest is summary-level.
            var recipientEmails = await ResolveRecipientEmailsAsync(roleLookup, options, cancellationToken);
            if (recipientEmails.Count == 0)
            {
                _logger.LogWarning(
                    "WeeklyBackdatedJournalDigestJob: No recipients resolved from configured role(s) — skipping run.");
                return;
            }

            var companies = await companyLookup.GetAllCompanyAsync();

            foreach (var company in companies)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var rows = await queryRepo.GetBackdatedJournalsForDigestAsync(
                        company.CompanyId, windowStart, windowEnd, cancellationToken);

                    if (rows.Count == 0)
                        continue;

                    var html = BuildDigestHtml(
                        company.CompanyName ?? $"Company {company.CompanyId}",
                        windowStart, windowEnd, rows);

                    foreach (var to in recipientEmails)
                    {
                        await mediator.Send(new SendEmailCommand
                        {
                            ToEmail     = to,
                            Subject     = $"BSOFT — Weekly Backdated Journal Digest ({company.CompanyName ?? company.CompanyId.ToString()})",
                            HtmlContent = html
                        }, cancellationToken);
                    }

                    _logger.LogInformation(
                        "WeeklyBackdatedJournalDigestJob: Sent digest for Company {Id} ({Count} backdated rows, {Recipients} recipients).",
                        company.CompanyId, rows.Count, recipientEmails.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "WeeklyBackdatedJournalDigestJob: Failed for Company {Id}.", company.CompanyId);
                }
            }
        }

        private static async Task<List<string>> ResolveRecipientEmailsAsync(
            IRoleUserLookup roleLookup,
            BackdateDigestOptions options,
            CancellationToken ct)
        {
            var emails = new List<string>();

            foreach (var roleId in new[] { options.CfoRoleId, options.FcRoleId })
            {
                if (roleId <= 0) continue;
                emails.AddRange(await roleLookup.GetEmailsByRoleAsync(roleId, ct));
            }

            emails.AddRange(options.CcEmails);

            return emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildDigestHtml(
            string companyName,
            DateTimeOffset windowStart,
            DateTimeOffset windowEnd,
            IReadOnlyList<LatePostingReportDto> rows)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style=\"font-family:Segoe UI, Arial, sans-serif;font-size:13px;color:#222;\">");
            sb.Append("<h2 style=\"color:#1f3a93;\">BSOFT — Weekly Backdated Journal Digest</h2>");
            sb.Append("<p><strong>Company:</strong> ").Append(System.Net.WebUtility.HtmlEncode(companyName)).Append("</p>");
            sb.Append("<p><strong>Window:</strong> ")
              .Append(windowStart.ToString("u", CultureInfo.InvariantCulture)).Append(" &mdash; ")
              .Append(windowEnd.ToString("u", CultureInfo.InvariantCulture)).Append("</p>");
            sb.Append("<p><strong>Backdated vouchers in window:</strong> ").Append(rows.Count).Append("</p>");

            sb.Append("<table cellpadding=\"6\" cellspacing=\"0\" border=\"1\" style=\"border-collapse:collapse;\">");
            sb.Append("<thead style=\"background:#f0f3f7;\"><tr>")
              .Append("<th>Voucher</th><th>Voucher Date</th><th>Posted At</th><th>Days Late</th>")
              .Append("<th>Period</th><th>Status</th><th>Reason</th><th>Posted By</th>")
              .Append("</tr></thead><tbody>");

            foreach (var r in rows)
            {
                sb.Append("<tr>");
                sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(r.VoucherNo ?? "(draft)")).Append("</td>");
                sb.Append("<td>").Append(r.VoucherDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append("</td>");
                sb.Append("<td>").Append(r.PostedAt?.ToString("u", CultureInfo.InvariantCulture) ?? "&mdash;").Append("</td>");
                sb.Append("<td style=\"text-align:right;\">").Append(r.DaysBackdated).Append("</td>");
                sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(r.AccountingPeriodName ?? "&mdash;")).Append("</td>");
                sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(r.StatusCode ?? "&mdash;")).Append("</td>");
                sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(r.BackdateReason ?? "")).Append("</td>");
                sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(r.PostedBy ?? "")).Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append("<p style=\"color:#888;font-size:11px;\">This digest is auto-generated by BSOFT (US-GL03-04). ")
              .Append("Drill-down available at /api/finance/Journal/late-posting-report.</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }
    }
}
