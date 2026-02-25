using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending
{
    public class GetAssetRecieptDtlPendingQuery   : IRequest<AssetTrasnferReceiptHdrPendingDto>
    {
         public int AssetTransferId { get; set; }
    }
}