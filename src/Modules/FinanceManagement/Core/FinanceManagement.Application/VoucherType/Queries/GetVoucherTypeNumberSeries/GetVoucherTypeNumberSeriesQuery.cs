using Contracts.Common;
using FinanceManagement.Application.VoucherType.Dto;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeNumberSeries
{
    // Backs the "Number Series & FY Reset" tab — next/last number per voucher type for a fiscal year.
    public class GetVoucherTypeNumberSeriesQuery : IRequest<ApiResponseDTO<List<VoucherTypeNumberSeriesDto>>>
    {
        public int FinancialYearId { get; set; }
        // CompanyId comes from the session token.
    }
}
