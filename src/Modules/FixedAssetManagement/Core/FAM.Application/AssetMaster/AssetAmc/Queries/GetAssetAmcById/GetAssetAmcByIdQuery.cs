using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmcById
{
    public class GetAssetAmcByIdQuery : IRequest<AssetAmcDto>
    {
        public int Id {get; set;}
    }
}