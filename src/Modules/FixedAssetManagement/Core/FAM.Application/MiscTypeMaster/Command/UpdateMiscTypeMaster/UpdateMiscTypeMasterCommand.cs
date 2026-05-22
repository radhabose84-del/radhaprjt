using MediatR;
using Contracts.Common;

namespace FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommand  : IRequest<bool>, IRequirePermission
    {

        public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        public byte IsActive { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
