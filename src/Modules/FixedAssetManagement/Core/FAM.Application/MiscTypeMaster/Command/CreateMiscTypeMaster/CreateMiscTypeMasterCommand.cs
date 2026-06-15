using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;
using Contracts.Common;

namespace FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<GetMiscTypeMasterDto>, IRequirePermission
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
       public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
