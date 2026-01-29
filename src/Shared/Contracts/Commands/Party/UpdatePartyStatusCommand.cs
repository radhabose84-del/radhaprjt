using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Commands.Party
{
    public class UpdatePartyStatusCommand
    {
        public Guid CorrelationId { get; set; }
        public List<int> PartyIds { get; set; } = new();
        public string PartyStatus { get; set; }
        public int StatusId { get; set; }
        
    }
}