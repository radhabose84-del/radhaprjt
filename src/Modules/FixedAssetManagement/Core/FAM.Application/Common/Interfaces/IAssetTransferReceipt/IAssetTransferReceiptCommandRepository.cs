using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetTransferReceipt
{
    public interface IAssetTransferReceiptCommandRepository
    {
        Task<int> CreateAsync(AssetTransferReceiptHdr assetTransferReceiptHdr,List<FAM.Domain.Entities.AssetMaster.AssetLocation> assetLocation);
    }
}