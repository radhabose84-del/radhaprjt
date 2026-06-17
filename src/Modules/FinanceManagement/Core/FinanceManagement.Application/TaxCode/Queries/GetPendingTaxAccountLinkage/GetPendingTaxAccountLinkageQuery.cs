using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetPendingTaxAccountLinkage
{
    // Linkages awaiting dual approval (StatusId = ApprovalStatus 'Pending').
    public class GetPendingTaxAccountLinkageQuery : IRequest<ApiResponseDTO<List<PendingTaxAccountLinkageDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
