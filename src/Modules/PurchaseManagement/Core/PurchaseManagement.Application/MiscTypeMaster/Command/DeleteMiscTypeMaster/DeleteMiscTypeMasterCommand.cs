using Contracts.Common;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace PurchaseManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>, IRequirePermission
    {
          public int Id { get; set; }
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
