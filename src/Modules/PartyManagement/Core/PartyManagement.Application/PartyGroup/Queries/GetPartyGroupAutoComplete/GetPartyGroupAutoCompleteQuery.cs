using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete
{
    public class GetPartyGroupAutoCompleteQuery : IRequest<List<PartyGroupAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}