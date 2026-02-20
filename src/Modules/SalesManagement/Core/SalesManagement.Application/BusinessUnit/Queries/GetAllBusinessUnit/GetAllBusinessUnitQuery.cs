#nullable disable

using Contracts.Common;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;

namespace SalesManagement.Application.BusinessUnit.Queries.GetAllBusinessUnit
{
    public class GetAllBusinessUnitQuery : IRequest<ApiResponseDTO<List<BusinessUnitDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
    }
}
