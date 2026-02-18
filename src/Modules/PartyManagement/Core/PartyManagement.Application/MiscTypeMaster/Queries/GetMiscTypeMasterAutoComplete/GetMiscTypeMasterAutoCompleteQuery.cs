using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery  :  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}