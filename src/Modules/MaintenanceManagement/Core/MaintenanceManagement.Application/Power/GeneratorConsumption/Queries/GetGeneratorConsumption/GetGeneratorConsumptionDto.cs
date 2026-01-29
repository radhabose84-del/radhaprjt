using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetGeneratorConsumption
{
    public class GetGeneratorConsumptionDto
    {
        public int Id { get; set; }
        public string? MachineCode { get; set; } 
        public string? MachineName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal RunningHours { get; set; }
        public decimal DieselConsumption { get; set; }
        public int UnitId { get; set; }
        public decimal OpeningEnergyReading { get; set; }
        public decimal ClosingEnergyReading { get; set; }
        public decimal Energy { get; set; }
        public string? Purpose { get; set; } 
        public string? CreatedByName { get; set; } 
        public DateTimeOffset CreatedDate { get; set; }
    }
}