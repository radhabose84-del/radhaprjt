#nullable disable
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel
{
    public class CreateSalesChannelCommand : IRequest<ApiResponseDTO<int>>
    {
        public string SalesChannelCode { get; set; }
        public string SalesChannelName { get; set; }
    }
}
