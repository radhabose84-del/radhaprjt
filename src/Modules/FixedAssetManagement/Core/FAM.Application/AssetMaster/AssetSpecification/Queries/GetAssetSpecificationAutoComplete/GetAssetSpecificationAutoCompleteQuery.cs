using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationAutoComplete
{
    public class GetAssetSpecificationAutoCompleteQuery : IRequest<List<AssetSpecificationJsonDto>>
    {
        public string? SearchPattern { get; set; }
    }
}