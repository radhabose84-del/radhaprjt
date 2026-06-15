using Contracts.Common;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace MaintenanceManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>, IRequirePermission
    {
          public int Id { get; set; }
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
