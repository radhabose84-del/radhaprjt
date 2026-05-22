using MediatR;
using Contracts.Common;

namespace FAM.Application.MiscMaster.Command.UpdateMiscMaster
{
    public class UpdateMiscMasterCommand : IRequest<bool>, IRequirePermission
    {
        
         
        public int Id { get; set; }
        public int MiscTypeId { get; set; }  
        public string? Code { get; set;}
        public string? Description { get; set;}
        public int SortOrder  { get; set;}
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
