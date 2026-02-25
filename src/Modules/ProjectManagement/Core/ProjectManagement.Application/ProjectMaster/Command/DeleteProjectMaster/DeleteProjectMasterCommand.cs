using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Command.DeleteProjectMaster
{
    public class DeleteProjectMasterCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteProjectMasterCommand(int id)
        {
            Id = id;
        }

    }
}