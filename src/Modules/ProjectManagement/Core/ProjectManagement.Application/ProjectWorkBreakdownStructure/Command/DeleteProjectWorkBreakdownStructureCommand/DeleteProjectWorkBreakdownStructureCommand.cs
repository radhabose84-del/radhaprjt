using MediatR;
using Contracts.Common;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand
{
    public class DeleteProjectWorkBreakdownStructureCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }

        public DeleteProjectWorkBreakdownStructureCommand(int id)
        {
            Id = id;
        }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
