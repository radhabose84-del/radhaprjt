using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Commands.Budget
{
    public class RollbackTransactionCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string Reason { get; set; } = default!;
        public string ModuleTypeName { get; set; } = default!;
    }
}