using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetByProject
{
    public class GetProjectWorkBreakdownStructureByProjectQuery   : IRequest<IReadOnlyList<ProjectWorkBreakdownStructureDto>>
    {
        public int ProjectId { get; set; }

        public GetProjectWorkBreakdownStructureByProjectQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}