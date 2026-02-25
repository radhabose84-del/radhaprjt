using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Autocomplete
{
    public class GetProjectWorkBreakdownStructureAutocompleteQuery  : IRequest<IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>>
    {
        public int ProjectId { get; set; }
        public string? SearchPattern { get; set; }

        public GetProjectWorkBreakdownStructureAutocompleteQuery(int projectId, string? searchPattern)
        {
            ProjectId = projectId;
            SearchPattern = searchPattern;
        }
    }
}