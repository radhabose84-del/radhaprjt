using FinanceManagement.Application.JournalMaster.Journal.Queries.GetLatePostingReport;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    /// <summary>
    /// US-GL03-04 / AC#3 — guards the late-posting report query inputs: paging is bounded, optional
    /// FK + date filters are sane, and SortBy/SortDirection are restricted to a safe allow-list (the
    /// Dapper repo concatenates them into ORDER BY — strict allow-list is the SQLi defence).
    /// </summary>
    public class GetLatePostingReportQueryValidator : AbstractValidator<GetLatePostingReportQuery>
    {
        private static readonly HashSet<string> AllowedSortBy =
            new(StringComparer.OrdinalIgnoreCase) { "PostedAt", "VoucherDate", "DaysBackdated" };

        private static readonly HashSet<string> AllowedSortDirection =
            new(StringComparer.OrdinalIgnoreCase) { "ASC", "DESC" };

        public GetLatePostingReportQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than zero.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200.");

            RuleFor(x => x.AccountingPeriodId)
                .GreaterThan(0).WithMessage("AccountingPeriodId must be greater than zero.")
                .When(x => x.AccountingPeriodId.HasValue);

            // Date range — when both supplied, From must not exceed To.
            RuleFor(x => x)
                .Must(q => !(q.FromDate.HasValue && q.ToDate.HasValue) || q.FromDate!.Value <= q.ToDate!.Value)
                .WithMessage("FromDate must be less than or equal to ToDate.");

            RuleFor(x => x.SortBy)
                .Must(s => AllowedSortBy.Contains(s!))
                .WithMessage("SortBy must be one of: PostedAt, VoucherDate, DaysBackdated.")
                .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

            RuleFor(x => x.SortDirection)
                .Must(s => AllowedSortDirection.Contains(s!))
                .WithMessage("SortDirection must be ASC or DESC.")
                .When(x => !string.IsNullOrWhiteSpace(x.SortDirection));
        }
    }
}
