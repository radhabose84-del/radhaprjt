using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringTemplateNow
{
    // "Generate" button — force-generate ONE template's JV now for the given voucher date. Everything else
    // (company from session, currency/exchange-rate per detail line, voucher type, accounts) is read from the
    // template header/detail master. The period is derived from VoucherDate and recorded done (idempotent).
    public sealed class GenerateRecurringTemplateNowCommand : IRequest<ApiResponseDTO<int>>
    {
        public int TemplateId { get; set; }
        public DateOnly VoucherDate { get; set; }      // the scheduled period date (e.g. 2026-07-15)
    }
}
