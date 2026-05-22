using MediatR;
using Contracts.Common;

namespace FAM.Application.Location.Command.DeleteAubLocation
{
    public class DeleteSubLocationCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
