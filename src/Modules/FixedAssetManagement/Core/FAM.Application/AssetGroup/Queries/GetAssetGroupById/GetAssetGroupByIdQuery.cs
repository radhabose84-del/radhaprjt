using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroupById
{
    public class GetAssetGroupByIdQuery : IRequest<AssetGroupDto>
    {
        public int Id { get; set; }
    }
}