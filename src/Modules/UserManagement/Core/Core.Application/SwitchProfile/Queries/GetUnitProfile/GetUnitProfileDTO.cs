using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.SwitchProfile.Queries.GetUnitProfile
{
    public class GetUnitProfileDTO
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? OldUnitId { get; set; }
        public int DivisionId { get; set; }
        public string? DivisionName { get; set; }
        public string? DivisionShortName { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }
}