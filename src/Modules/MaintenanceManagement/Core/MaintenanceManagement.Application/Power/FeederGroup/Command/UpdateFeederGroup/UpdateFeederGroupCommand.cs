using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup
{
    public class UpdateFeederGroupCommand  : IRequest<bool>, IRequirePermission
    { 
        public int Id { get; set; }
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }  
        public int  UnitId { get; set; }      
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
