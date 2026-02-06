using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Command.DeletePartyGroup
{
    public class DeletePartyGroupCommand : IRequest<bool>
    {
      public int Id { get; set; }   
    }
}