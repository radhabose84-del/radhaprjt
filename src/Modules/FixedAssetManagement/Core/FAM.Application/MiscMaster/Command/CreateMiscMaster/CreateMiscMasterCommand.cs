using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;
using Contracts.Common;

namespace FAM.Application.MiscMaster.Command.CreateMiscMaster
{
    public class CreateMiscMasterCommand : IRequest<GetMiscMasterDto>, IRequirePermission
    {

         public int MiscTypeId { get; set; }  
        public string? Code { get; set;}
        public string? Description { get; set;}
       
        
         public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
