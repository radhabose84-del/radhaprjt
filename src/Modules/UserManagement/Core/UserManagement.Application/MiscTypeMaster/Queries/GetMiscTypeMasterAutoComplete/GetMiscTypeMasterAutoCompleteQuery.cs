using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery :  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>
    {
          public string? SearchPattern { get; set; }
        
    }
}