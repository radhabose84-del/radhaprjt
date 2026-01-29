using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending
{
    public class AssetTrasnferReceiptHdrPendingDto
    {
        public int AssetTransferId { get; set; }        
        public DateTimeOffset DocDate { get; set; }
        public string? TransferType { get; set; } 
        public string? FromUnitname { get; set; }  
        public string? ToUnitname { get; set; } 
        public string? FromDepartment { get; set; }
        public string? ToDepartment { get; set; }
        public string? FromCustodianName { get; set; }
        public string? ToCustodianName { get; set; }
        public string? Sdcno { get; set; }
        public string? GatePassNo  { get; set; }
        public string? Remarks { get; set; }
        public List<AssetTransferReceiptDtlPendingDto>? AssetTransferPendingDtl { get; set; }

    }
}