using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ReorderLineItem
{
    public class ReorderLineItemCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int LineItemId { get; set; }
        public int Direction { get; set; }   // 1 = move up, 2 = move down

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
