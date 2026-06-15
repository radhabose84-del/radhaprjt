using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.DutyMaster.Command.Create
{
    public class CreateDutyMasterCommand : IRequest<int>, IRequirePermission
    {
        public DutyMasterDto Model { get; set; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
