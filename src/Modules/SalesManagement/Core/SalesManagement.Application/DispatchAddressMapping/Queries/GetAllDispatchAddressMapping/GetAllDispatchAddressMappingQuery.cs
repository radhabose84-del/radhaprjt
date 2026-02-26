using Contracts.Common;
using MediatR;
using SalesManagement.Application.DispatchAddressMapping.Dto;

namespace SalesManagement.Application.DispatchAddressMapping.Queries.GetAllDispatchAddressMapping
{
    public class GetAllDispatchAddressMappingQuery : IRequest<ApiResponseDTO<List<DispatchAddressMappingDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? PartyId { get; set; }
    }
}
