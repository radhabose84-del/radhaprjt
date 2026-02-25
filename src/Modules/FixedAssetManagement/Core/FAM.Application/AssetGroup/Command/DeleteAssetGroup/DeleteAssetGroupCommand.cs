using MediatR;

namespace FAM.Application.AssetGroup.Command.DeleteAssetGroup
{
    public class DeleteAssetGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}