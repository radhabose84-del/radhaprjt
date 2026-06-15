using MediatR;
using Contracts.Common;

namespace FAM.Application.UOM.Command.DeleteUOM
{
    public class DeleteUOMCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
