using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
{
    public class CreatePartyMasterCommand : IRequest<int>
    {
        public CreatePartyMasterDto PartyMaster { get; set; } = null!;
    }
}