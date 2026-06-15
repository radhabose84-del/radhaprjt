using FAM.Application.Location.Queries.GetLocations;
using MediatR;
using Contracts.Common;

namespace FAM.Application.Location.Command.DeleteLocation
{
    public class DeleteLocationCommand : IRequest<LocationDto>, IRequirePermission
    {
        public int Id { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
