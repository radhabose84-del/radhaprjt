#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetAllSalesOffice
{
    public class GetAllSalesOfficeQuery : IRequest<ApiResponseDTO<List<SalesOfficeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
    }
}
