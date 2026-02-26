using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesContact.Dto;

namespace SalesManagement.Application.SalesContact.Queries.GetAllSalesContact
{
    public class GetAllSalesContactQuery : IRequest<ApiResponseDTO<List<SalesContactDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
