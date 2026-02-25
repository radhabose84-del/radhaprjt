using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt
{
    public class CreateAssetTransferReceiptCommand   : IRequest<int>
    {
          public AssetTransferReceiptHdrDto? AssetTransferReceiptHdrDto { get; set; }
    }
}