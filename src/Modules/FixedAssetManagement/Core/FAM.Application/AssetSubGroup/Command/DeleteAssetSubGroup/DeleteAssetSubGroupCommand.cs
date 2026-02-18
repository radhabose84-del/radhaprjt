using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup
{
    public class DeleteAssetSubGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}