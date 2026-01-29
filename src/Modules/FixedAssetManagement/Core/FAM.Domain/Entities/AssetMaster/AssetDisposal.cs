using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetDisposal : BaseEntity
    {
        public int AssetId { get; set; } 
        public AssetMasterGenerals AssetMasterDisposal  { get; set; } = null!;
        public int AssetPurchaseId { get; set; } 
        public AssetPurchaseDetails AssetPurchaseDetails   { get; set; } = null!;
        public DateOnly DisposalDate { get; set; }
        public int? DisposalType { get; set; }        
        public MiscMaster AssetMiscDisposalType { get; set; } = null!;  
        public string? DisposalReason { get; set; }
        public decimal? DisposalAmount { get; set; }

    }
}