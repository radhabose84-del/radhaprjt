using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetLinkageChangeAudit
{
    public class GetLinkageChangeAuditQuery : IRequest<ApiResponseDTO<List<TaxAccountLinkageDto>>>
    {
        public string? Status { get; set; }
        public int? CompanyId { get; set; }
    }
}
