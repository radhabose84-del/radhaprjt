using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQuery  :  IRequest<List<GetMiscMasterAutoCompleteDto>>
    {


          public string? MiscTypeCode { get; set; }
         public string? MiscTypeName { get; set; }
        
    }
}