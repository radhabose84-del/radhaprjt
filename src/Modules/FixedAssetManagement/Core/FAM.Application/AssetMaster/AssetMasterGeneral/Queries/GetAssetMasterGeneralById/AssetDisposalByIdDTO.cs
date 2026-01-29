using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetDisposalByIdDTO
    {
        public int Id { get; set; }
        public string? DisposalType { get; set; }
        public string? DisposalDate { get; set; }
        public string? DisposalReason { get; set; }
        public string? DisposalAmount { get; set; }
        public int DisposalTypeId { get; set; }
        public int AssetPurchaseId { get; set; }
    }
}