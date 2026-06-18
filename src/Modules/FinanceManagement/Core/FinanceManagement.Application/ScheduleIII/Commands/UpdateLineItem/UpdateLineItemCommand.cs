using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem
{
    public class UpdateLineItemCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string? LineName { get; set; }
        public string? NoteReference { get; set; }
        public int IsSplitLine { get; set; }   // 0 = No, 1 = Yes
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
        // LineCode is immutable — excluded from update.

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
