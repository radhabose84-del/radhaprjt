using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupAutoComplete
{
    public class GetAssetSubGroupAutoCompleteQuery : IRequest<List<AssetSubGroupAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}