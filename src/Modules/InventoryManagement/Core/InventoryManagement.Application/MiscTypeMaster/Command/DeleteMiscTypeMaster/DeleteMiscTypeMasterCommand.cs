using Contracts.Common;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace InventoryManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>, IRequirePermission
    {
          public int Id { get; set; }
        
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
