using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Command
{
    public class CreateGeneratorConsumptionCommand : IRequest<int>
    {
        public int GeneratorId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal DieselConsumption { get; set; }
        public decimal OpeningEnergyReading { get; set; }
        public decimal ClosingEnergyReading { get; set; }
        public int? PurposeId { get; set; }  
        public int UnitId { get; set; }

    }
}