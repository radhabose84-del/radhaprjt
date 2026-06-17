using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionAccountLinkage
{
    public class GetAllGstrSectionAccountLinkageQuery : IRequest<ApiResponseDTO<List<GstrSectionAccountLinkageDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
