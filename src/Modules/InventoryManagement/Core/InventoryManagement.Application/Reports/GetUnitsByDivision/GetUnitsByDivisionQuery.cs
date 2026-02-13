using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;
using MediatR;

namespace InventoryManagement.Application.Reports.GetUnitsByDivision
{
    public class GetUnitsByDivisionQuery : IRequest<List<DivisionUnitLookupDto>>
    {
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
    }
}
