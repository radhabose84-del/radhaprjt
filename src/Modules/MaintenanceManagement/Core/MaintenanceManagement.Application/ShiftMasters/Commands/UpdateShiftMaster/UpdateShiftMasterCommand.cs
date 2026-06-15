using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster
{
    public class UpdateShiftMasterCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }
        public string ShiftCode { get; set; } = default!;
        public string ShiftName { get; set; } = default!;
        public DateOnly EffectiveDate { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
