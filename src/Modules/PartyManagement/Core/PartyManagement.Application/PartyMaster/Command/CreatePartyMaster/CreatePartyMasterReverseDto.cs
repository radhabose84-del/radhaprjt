using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;

namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
{
    public class CreatePartyMasterReverseDto
    {
        public PartyMasterWorkFlowDto? Header { get; set; }
        public ICollection<PartyMasterWorkFlowDto>? Lines { get; set; }
    }
    
}