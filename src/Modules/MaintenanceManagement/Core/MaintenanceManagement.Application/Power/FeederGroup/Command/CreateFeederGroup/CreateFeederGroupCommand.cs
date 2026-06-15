using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup
{
    public class CreateFeederGroupCommand : IRequest<int>, IRequirePermission
    {
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }
        public int UnitId { get; set; }
       
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
