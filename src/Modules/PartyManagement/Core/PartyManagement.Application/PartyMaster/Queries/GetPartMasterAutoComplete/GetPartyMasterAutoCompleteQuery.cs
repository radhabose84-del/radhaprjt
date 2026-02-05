using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class GetPartyMasterAutoCompleteQuery : IRequest<List<GetPartyMasterAutoCompleteDto>>
    {
        public List<int>? PartyTypeIds { get; set; } // For multi-select filter
        public string? SearchPattern { get; set; }
    }
}