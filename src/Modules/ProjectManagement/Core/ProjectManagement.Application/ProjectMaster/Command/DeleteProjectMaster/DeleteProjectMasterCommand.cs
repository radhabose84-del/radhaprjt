using MediatR;
using Contracts.Common;

namespace ProjectManagement.Application.ProjectMaster.Command.DeleteProjectMaster
{
    public class DeleteProjectMasterCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }

        public DeleteProjectMasterCommand(int id)
        {
            Id = id;
        }

        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
