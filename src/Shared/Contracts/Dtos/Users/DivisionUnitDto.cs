using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class DivisionUnitDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}