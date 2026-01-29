using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Commands.Maintenance
{
    public class RollbackNextScheduleCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int SchedulerId { get; set; }
    }
}