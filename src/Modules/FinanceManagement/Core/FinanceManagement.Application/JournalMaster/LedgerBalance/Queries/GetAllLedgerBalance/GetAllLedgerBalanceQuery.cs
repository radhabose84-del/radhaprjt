using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.LedgerBalance.Queries.GetAllLedgerBalance
{
    // Period ledger balances enriched with GL account / account type / account group. CompanyId from session.
    public class GetAllLedgerBalanceQuery : IRequest<ApiResponseDTO<List<LedgerBalanceDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int? AccountingPeriodId { get; set; }
        public int? FinancialYearId { get; set; }
        public int? GlAccountId { get; set; }
        public int? AccountTypeId { get; set; }
        public int? AccountGroupId { get; set; }
        public int? CostCentreId { get; set; }
        public string? SearchTerm { get; set; }
    }
}
