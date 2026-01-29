using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Purchase;
using Contracts.Dtos.Workflow;
using MassTransit;

namespace Contracts.Commands.Workflow
{
    public class RollbackApprovedRejectStatus : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int IndentId { get; set; }
        public string Reason { get; set; }
        public ICollection<UpdateLineStatusDto> RollbackApprovedRejected { get; set; }
        
    }
}