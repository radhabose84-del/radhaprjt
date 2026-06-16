using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionMapping
{
    public class GetAllGstrSectionMappingQuery : IRequest<ApiResponseDTO<List<GstrSectionMappingDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? CompanyId { get; set; }
    }
}
