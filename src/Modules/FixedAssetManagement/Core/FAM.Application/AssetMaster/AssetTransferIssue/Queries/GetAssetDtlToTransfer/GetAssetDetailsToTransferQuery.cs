using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer
{
    public class GetAssetDetailsToTransferQuery : IRequest<GetAssetDetailsToTransferHdrDto>
    {
         public int AssetId { get; set; }
    }
}