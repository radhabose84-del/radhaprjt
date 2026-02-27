using Contracts.Common;
using MediatR;
using SalesManagement.Application.DispatchAddressMaster.Dto;

namespace SalesManagement.Application.DispatchAddressMaster.Queries.GetAllDispatchAddressMaster
{
    public class GetAllDispatchAddressMasterQuery : IRequest<ApiResponseDTO<List<DispatchAddressMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
