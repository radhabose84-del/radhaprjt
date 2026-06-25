using Contracts.Common;
using FinanceManagement.Application.CoaReport.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetAccountUsageReport
{
    // US-GL02-15 (AC2/AC3) — never-posted + "no posting in N months" deactivation-candidate report.
    public class GetAccountUsageReportQuery : IRequest<ApiResponseDTO<List<AccountUsageItemDto>>>
    {
        // Defaults to 12 months (AC3 "no transactions in 12 months").
        public int MonthsSincePosting { get; set; } = 12;
    }
}
