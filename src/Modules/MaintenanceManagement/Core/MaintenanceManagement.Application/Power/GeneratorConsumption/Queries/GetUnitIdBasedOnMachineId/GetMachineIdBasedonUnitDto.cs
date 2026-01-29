using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetUnitIdBasedOnMachineId
{
    public class GetMachineIdBasedonUnitDto
    {
        public int Id { get; set; }
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }
    }
}