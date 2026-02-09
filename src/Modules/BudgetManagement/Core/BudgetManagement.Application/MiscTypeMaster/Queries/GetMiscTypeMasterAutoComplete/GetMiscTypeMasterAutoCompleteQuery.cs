using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.Common.HttpResponse;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery :  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>             
    {
         public string? SearchPattern { get; set; }
    }
}