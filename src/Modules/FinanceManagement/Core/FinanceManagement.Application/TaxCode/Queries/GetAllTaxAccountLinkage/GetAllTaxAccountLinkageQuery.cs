using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllTaxAccountLinkage
{
    public class GetAllTaxAccountLinkageQuery : IRequest<ApiResponseDTO<List<TaxAccountLinkageDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? CompanyId { get; set; }
    }
}
