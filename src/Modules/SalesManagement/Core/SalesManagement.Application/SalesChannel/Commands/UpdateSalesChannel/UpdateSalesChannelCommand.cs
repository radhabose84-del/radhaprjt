using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel
{
    public class UpdateSalesChannelCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SalesChannelName { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
