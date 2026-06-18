using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem
{
    public class CreateLineItemCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int SectionId { get; set; }
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? NoteReference { get; set; }
        public int IsSplitLine { get; set; }   // 0 = No, 1 = Yes

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
