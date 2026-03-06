using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace BackgroundService.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQuery  :  IRequest<List<GetMiscMasterAutoCompleteDto>>
    {

          public string? SearchPattern { get; set; }
          public string? MiscTypeCode { get; set; }
        
    }
}