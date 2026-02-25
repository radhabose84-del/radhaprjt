using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand
{
    public class DeleteProjectWorkBreakdownStructureCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteProjectWorkBreakdownStructureCommand(int id)
        {
            Id = id;
        }
    }
}