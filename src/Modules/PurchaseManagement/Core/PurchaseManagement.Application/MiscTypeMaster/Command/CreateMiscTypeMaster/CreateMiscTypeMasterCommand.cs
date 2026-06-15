using Contracts.Common;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace PurchaseManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>, IRequirePermission
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
        
       public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
