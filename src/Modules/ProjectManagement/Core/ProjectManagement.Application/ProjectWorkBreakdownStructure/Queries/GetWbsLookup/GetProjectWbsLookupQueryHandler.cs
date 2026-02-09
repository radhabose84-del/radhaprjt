using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup
{
    public class GetProjectWbsLookupQueryHandler : IRequestHandler<GetProjectWbsLookupQuery, List<ProjectWbsLookupDto>>
    {
        private readonly IProjectWorkBreakdownStructureQueryRepository _repo;
        private readonly IIPAddressService _ipAddressService;

        public GetProjectWbsLookupQueryHandler(IProjectWorkBreakdownStructureQueryRepository repo, IIPAddressService ipAddressService)
        {
            _repo = repo;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<ProjectWbsLookupDto>> Handle(
            GetProjectWbsLookupQuery request,
            CancellationToken ct)
        {
            var unitId = _ipAddressService.GetUnitId();

            return await _repo.GetWbsLookupAsync(request.ProjectId, ct);
        }
    }
}