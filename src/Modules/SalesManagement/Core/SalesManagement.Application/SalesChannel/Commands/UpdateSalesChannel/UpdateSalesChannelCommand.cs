using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel
{
    public class UpdateSalesChannelCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string SalesChannelName { get; set; } = null!;
        public int IsActive { get; set; }
    }
}
