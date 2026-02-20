#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById
{
    public class GetSalesChannelByIdQuery : IRequest<ApiResponseDTO<SalesChannelDto>>
    {
        public int Id { get; set; }
    }
}
