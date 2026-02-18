using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Contracts.Dtos.Common;
using Contracts.Dtos.Purchase;
using MassTransit;

namespace Contracts.Commands.Inventory
{
    public class UpdateApprovedRejectedInventoryCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; } = default!;
        public List<PartyRefDto> PartyContacts { get; set; } = new();
        public List<JsonElement> DynamicFields { get; set; } = new();
    }
}