using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringJournals
{
    // US-GL01-11B — manual trigger to instantiate due recurring templates for a period (the same service the
    // Hangfire period-open job will call). Returns the number of journals generated. CompanyId comes from session.
    public sealed class GenerateRecurringJournalsCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int BaseCurrencyId { get; set; }      // currency the generated lines post in
        public string? Period { get; set; }          // e.g. "2026-06" — idempotency key per (company, template, period)
        public DateOnly VoucherDate { get; set; }    // voucher date for generated journals (must be an OPEN period)

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
