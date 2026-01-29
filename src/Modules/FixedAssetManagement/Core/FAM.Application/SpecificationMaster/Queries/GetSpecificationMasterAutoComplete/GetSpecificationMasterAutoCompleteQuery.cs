using FAM.Application.Common.HttpResponse;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using MediatR;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterAutoComplete
{
    public class GetSpecificationMasterAutoCompleteQuery : IRequest<List<SpecificationMasterAutoCompleteDTO>>
    {
        public int? AssetGroupId { get; set; }
         public string? SearchPattern { get; set; }
    }
}