using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption
{
    public class CreatePowerConsumptionCommand : IRequest<int>
    {
        public int FeederTypeId { get; set; }
        public int FeederId { get; set; }
        public int UnitId { get; set; }
        public decimal OpeningReading { get; set; }
        public decimal ClosingReading { get; set; }
        
    }
}