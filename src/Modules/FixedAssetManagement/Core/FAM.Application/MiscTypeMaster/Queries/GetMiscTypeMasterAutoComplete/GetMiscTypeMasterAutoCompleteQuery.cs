using MediatR;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;

namespace FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery  :  IRequest<List<GetMiscTypeMasterAutocompleteDto>>
    {
          public string? SearchPattern { get; set; }
    }
}