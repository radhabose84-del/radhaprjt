using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster
{
    public class UpdatePartyMasterCommand  : IRequest<bool>
    {
         public UpdatePartyMasterDto? UpdatePartyMaster { get; set; }
    }
}