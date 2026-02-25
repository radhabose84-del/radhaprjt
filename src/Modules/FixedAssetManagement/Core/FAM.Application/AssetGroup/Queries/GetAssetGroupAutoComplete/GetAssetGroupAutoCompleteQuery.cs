using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroupAutoComplete
{
    public class GetAssetGroupAutoCompleteQuery : IRequest<List<AssetGroupAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
       // public int AssetGroupId { get; set; }
    }
}