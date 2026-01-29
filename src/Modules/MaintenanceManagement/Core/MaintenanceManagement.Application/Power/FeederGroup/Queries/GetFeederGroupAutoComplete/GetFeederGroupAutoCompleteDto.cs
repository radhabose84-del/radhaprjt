using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete
{
    public class GetFeederGroupAutoCompleteDto
    {
        public int Id { get; set; }
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }

    }
}