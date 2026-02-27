using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesLead.Dto;

namespace SalesManagement.Application.SalesLead.Queries.GetAllSalesLead
{
    public class GetAllSalesLeadQuery : IRequest<ApiResponseDTO<List<SalesLeadDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
