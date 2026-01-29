using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete
{
    public class GetFeederAutoCompleteDto
    {
        public int Id { get; set; }
        public string? FeederCode { get; set; }
        public string? FeederName { get; set; }
    }
}