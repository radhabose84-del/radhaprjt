using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Entity.Queries.GetCompanyBasedUnit
{
    public class GetCompanyBasedUnitDto
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int CompanyId { get; set; }
       
    }
}