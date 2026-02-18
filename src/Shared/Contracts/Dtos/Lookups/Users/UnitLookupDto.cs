using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class UnitLookupDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public string ShortName { get; set; } = default!;
        public string UnitHeadName { get; set; } = default!;
        public string OldUnitId { get; set; } = default!;
        public int? SpindlesCapacity { get; set; }

    }
}