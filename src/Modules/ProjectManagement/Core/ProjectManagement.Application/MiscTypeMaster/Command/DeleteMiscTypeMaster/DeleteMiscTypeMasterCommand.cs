using Contracts.Common;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>, IRequirePermission
    {
          public int Id { get; set; }
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
