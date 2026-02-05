using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery:  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>             
    {
         public string? SearchPattern { get; set; }
        
    }
}