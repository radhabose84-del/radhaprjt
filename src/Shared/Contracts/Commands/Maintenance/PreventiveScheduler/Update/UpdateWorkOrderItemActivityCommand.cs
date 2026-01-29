using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Commands.Maintenance.PreventiveScheduler.Update
{
    public class UpdateWorkOrderItemActivityCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}