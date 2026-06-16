using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem
{
    public class UpdateLineItemCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? LineName { get; set; }
        public string? SubClassification { get; set; }
        public string? NoteReference { get; set; }
        public int DisplayOrder { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
        // LineCode is immutable — excluded from update.

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
