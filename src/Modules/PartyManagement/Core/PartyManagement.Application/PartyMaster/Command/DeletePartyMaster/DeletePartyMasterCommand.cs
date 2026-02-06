using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Command.DeletePartyMaster
{
    public class DeletePartyMasterCommand : IRequest<bool>
    {
        public int Id { get; set; }   
    }
}