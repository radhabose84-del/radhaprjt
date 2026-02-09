using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetByProject;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWBSByProject
{
    public class GetProjectWorkBreakdownStructureByProjectQueryHandler  : IRequestHandler<GetProjectWorkBreakdownStructureByProjectQuery, IReadOnlyList<ProjectWorkBreakdownStructureDto>>
    {
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepository;

        public GetProjectWorkBreakdownStructureByProjectQueryHandler(
            IProjectWorkBreakdownStructureQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<ProjectWorkBreakdownStructureDto>> Handle(
            GetProjectWorkBreakdownStructureByProjectQuery request,
            CancellationToken cancellationToken)
        {
            // Dapper repository call - returns all WBS under this ProjectId
            var result = await _queryRepository.GetByProjectAsync(request.ProjectId);
            return result;
        }
    }
}