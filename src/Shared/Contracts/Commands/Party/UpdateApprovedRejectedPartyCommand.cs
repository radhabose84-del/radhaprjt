using System;
using System.Collections.Generic;
using Contracts.Dtos.Common;
using Contracts.Dtos.Purchase;
using MassTransit;

namespace Contracts.Commands.Party
{
    public class UpdateApprovedRejectedPartyCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; }
        public string Status { get; set; }
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; }
        public List<PartyRefDto> PartyContacts { get; set; } = new();
    }
}