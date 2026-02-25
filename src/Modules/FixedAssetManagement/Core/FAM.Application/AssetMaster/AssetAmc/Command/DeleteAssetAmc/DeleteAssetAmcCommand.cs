using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc
{
    public class DeleteAssetAmcCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}