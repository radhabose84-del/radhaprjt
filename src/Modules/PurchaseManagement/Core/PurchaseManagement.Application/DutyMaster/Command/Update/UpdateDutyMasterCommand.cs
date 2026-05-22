using PurchaseManagement.Application.DutyMaster;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Command.Update
{
    public class UpdateDutyMasterCommand : IRequest<bool>, IRequirePermission
    {
        public DutyMasterDto Model { get; set; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
