using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetTransferReceiptHdr
    {
        public int Id {get;set;}
        public int AssetTransferId { get; set; }        
        public AssetTransferIssueHdr AssetTransferIssueHdr { get; set; } 
        public DateTimeOffset DocDate { get; set; }  
        public string? Sdcno { get; set; }
        public string? GatePassNo  { get; set; }
        public int? AuthorizedBy { get; set; }
        public DateTimeOffset? AuthorizedDate { get; set; }
        public string? AuthorizedByName { get; set; }
        public string? AuthorizedIP { get; set; }
        public string? Remarks { get; set; }
        public ICollection<AssetTransferReceiptDtl>? AssetTransferReceiptDtl { get; set; }


    }
}