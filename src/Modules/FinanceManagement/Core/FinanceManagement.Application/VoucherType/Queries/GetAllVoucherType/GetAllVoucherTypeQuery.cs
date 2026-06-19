using Contracts.Common;
using FinanceManagement.Application.VoucherType.Dto;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetAllVoucherType
{
    public class GetAllVoucherTypeQuery : IRequest<ApiResponseDTO<List<VoucherTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        // CompanyId comes from the session token. FinancialYearId is optional — the FY whose
        // "Next No. (this FY)" the list shows; when null the repo resolves the current FY by date.
        public int? FinancialYearId { get; set; }
    }
}
