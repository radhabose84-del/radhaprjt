using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAllAccountingPeriod
{
    public class GetAllAccountingPeriodQuery : IRequest<ApiResponseDTO<List<AccountingPeriodDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        // CompanyId comes from the session token. FinancialYearId is an optional filter.
        public int? FinancialYearId { get; set; }
    }
}
