using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt
{
    public class CreateAssetTransferReceiptCommand   : IRequest<int>, IRequirePermission
    {
          public AssetTransferReceiptHdrDto? AssetTransferReceiptHdrDto { get; set; }
          public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
