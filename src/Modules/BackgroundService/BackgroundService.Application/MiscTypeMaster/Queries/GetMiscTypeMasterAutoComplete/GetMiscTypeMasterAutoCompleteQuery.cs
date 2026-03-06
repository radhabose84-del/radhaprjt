using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery:  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>             
    {
         public string? SearchPattern { get; set; }
        
    }
}