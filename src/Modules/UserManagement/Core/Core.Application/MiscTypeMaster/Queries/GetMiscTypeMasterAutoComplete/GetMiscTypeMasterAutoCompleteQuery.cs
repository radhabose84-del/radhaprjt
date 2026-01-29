using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace Core.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery :  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>
    {
          public string? SearchPattern { get; set; }
        
    }
}