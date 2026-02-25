using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById
{
    public class GetAssetReceiptDetailsByIdQuery : IRequest<List<AssetReceiptDetailsByIdDto>>
    {
        public int AssetReceiptId { get; set; }
    }
}