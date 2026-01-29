using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Application.Notification.Common.HttpResponse;
using MediatR;

namespace BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery:  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>             
    {
         public string? SearchPattern { get; set; }
        
    }
}