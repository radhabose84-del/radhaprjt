using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetTransferReceipt
{
    public interface IAssetTransferReceiptQueryRepository
    {
        Task<(List<AssetTransferReceiptPendingDto>, int)> GetAllPendingAssetTransferAsync(
        int PageNumber, 
        int PageSize, 
        int? AssetTransferId,
        string? SearchTerm, 
        DateTimeOffset? FromDate, 
        DateTimeOffset? ToDate);
        
        //Task<List<AssetTrasnferReceiptHdrPendingDto>> GetAllPendingAssetTransferDtlAsync(int assetTransferId);

         Task<AssetTrasnferReceiptHdrPendingDto?> GetAssetTransferByIdAsync(int assetTransferId);

        Task<(List<AssetReceiptDetailsDto>, int)> GetAllAssetReceiptDetails(
        int PageNumber, 
        int PageSize, 
        string? SearchTerm, 
        DateTimeOffset? FromDate, 
        DateTimeOffset? ToDate);

        Task<List<AssetReceiptDetailsByIdDto>> GetByAssetReceiptId(int AssetReceiptId);
     
        Task<AssetTransferDto?> GetByAssetTransferId(int assetTransferId);
        



    }
}