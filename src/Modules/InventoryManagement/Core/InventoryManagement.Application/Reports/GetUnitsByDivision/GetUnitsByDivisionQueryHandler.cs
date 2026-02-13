using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MediatR;

namespace InventoryManagement.Application.Reports.GetUnitsByDivision
{
    public class GetUnitsByDivisionQueryHandler : IRequestHandler<GetUnitsByDivisionQuery, List<DivisionUnitLookupDto>>
    {
        private readonly IDivisionUnitLookup _divisionUnitLookup;
        public GetUnitsByDivisionQueryHandler(IDivisionUnitLookup divisionUnitLookup)
        {
            _divisionUnitLookup = divisionUnitLookup;
        }
          public async Task<List<DivisionUnitLookupDto>> Handle(
            GetUnitsByDivisionQuery request,
            CancellationToken cancellationToken)
        {
            return await _divisionUnitLookup.GetUnitsByDivisionAsync(
                request.CompanyId,
                request.DivisionId,
                cancellationToken);
        }

    }
}
