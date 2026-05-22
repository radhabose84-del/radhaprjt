using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster
{
    public class DeleteHSNMasterCommand: IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
