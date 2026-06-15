using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework
{
    public class RequestReworkCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int FeedbackId { get; set; }
        public string? ReworkReason { get; set; }
    }
}
