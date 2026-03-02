using Contracts.Common;
using MediatR;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Queries.GetAllCustomerVisit
{
    public class GetAllCustomerVisitQuery : IRequest<ApiResponseDTO<List<CustomerVisitDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
