using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposalById
{
    public class GetAssetDisposalByIdQuery : IRequest<AssetDisposalDto>
    {
        public int Id { get; set; }
    }
}