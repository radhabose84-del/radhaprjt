#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.SalesGroup.Queries.GetAllSalesGroup
{
    public class GetAllSalesGroupQuery : IRequest<ApiResponseDTO<List<SalesGroupDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
    }
}
