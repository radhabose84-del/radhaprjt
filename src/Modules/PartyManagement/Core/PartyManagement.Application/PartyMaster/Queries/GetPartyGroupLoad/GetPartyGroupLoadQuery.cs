using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad
{
    public class GetPartyGroupLoadQuery  : IRequest<List<PartyGroupLoadDto>>
    {
         public List<int>? GroupTypeIds { get; set; } // For multi-select filter
    }
}