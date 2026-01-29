using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById
{
    public class GetFeederSubFeederDto
    {
        public int Id { get; set; }
        public string? FeederCode { get; set; }
        public string? FeederName { get; set; }
    }
}