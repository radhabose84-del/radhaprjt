using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster
{
    public class UpdateActivityCheckListMasterCommand : IRequest<bool>, IRequirePermission
    {
       public int Id { get; set; }
       public int ActivityID { get; set; }       
       public string? ActivityChecklist { get; set; }
       public int UnitId { get; set; }
       public byte IsActive { get; set; }
        
       public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
