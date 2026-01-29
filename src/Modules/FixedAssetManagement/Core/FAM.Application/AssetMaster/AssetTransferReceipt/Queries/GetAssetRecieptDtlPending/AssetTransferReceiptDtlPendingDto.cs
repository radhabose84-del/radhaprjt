using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById
{
    public class AssetTransferReceiptDtlPendingDto
    {
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public int AssetId { get; set; }  
    }
}