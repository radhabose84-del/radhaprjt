using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageChangeAudit
{
    // Change Audit grid — every tax-code change request (old → new) with its approval status
    // (Approved / Pending / Rejected), reason, approvers, and effective-from date.
    public class GetTaxAccountLinkageChangeAuditQuery : IRequest<ApiResponseDTO<List<PendingTaxAccountLinkageDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? StatusId { get; set; }   // optional filter: ApprovalStatus (Pending / Approved / Rejected)
    }
}
