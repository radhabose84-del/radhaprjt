using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetCompanyBasedUnit
{
    public class GetCompanyBasedUnitQuery : IRequest<List<GetCompanyBasedUnitDto>>
    {
       public List<int>? CompanyIds { get; set; } // For multi-select filter
    }
}