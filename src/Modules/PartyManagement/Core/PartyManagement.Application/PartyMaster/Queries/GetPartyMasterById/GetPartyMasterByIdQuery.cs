using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById
{
    public class GetPartyMasterByIdQuery : IRequest<PartyMasterDto>
    {
        public int PartyId { get; set; }
    }
}