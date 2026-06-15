using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand  : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>, IRequirePermission
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
       public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
