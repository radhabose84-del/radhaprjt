using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Autocomplete
{
    public class GetProjectWorkBreakdownStructureAutocompleteQueryHandler  : IRequestHandler<GetProjectWorkBreakdownStructureAutocompleteQuery, IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>>
    {
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepository;

        public GetProjectWorkBreakdownStructureAutocompleteQueryHandler(
            IProjectWorkBreakdownStructureQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>> Handle(
            GetProjectWorkBreakdownStructureAutocompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetAutocompleteAsync(
                request.ProjectId,
                request.SearchPattern
            );

            return result;
        }
    }
}