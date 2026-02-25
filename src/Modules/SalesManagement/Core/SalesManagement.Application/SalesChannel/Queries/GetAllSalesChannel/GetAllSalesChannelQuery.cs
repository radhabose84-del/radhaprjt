using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetAllSalesChannel
{
    public class GetAllSalesChannelQuery : IRequest<ApiResponseDTO<List<SalesChannelDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
