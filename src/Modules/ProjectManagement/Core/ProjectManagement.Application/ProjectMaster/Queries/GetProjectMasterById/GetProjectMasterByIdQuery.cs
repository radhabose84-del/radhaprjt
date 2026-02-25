using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById
{
    public class GetProjectMasterByIdQuery:  IRequest<GetProjectMasterDto>
    {
        public int Id { get; set; }
        
    }
}