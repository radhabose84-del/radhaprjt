using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetPurchaseDetailDTO
    {
        public int Id { get; set; }
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public string? UnitName { get; set; }
        public string? SourceName { get; set; }
        public int GrnNo { get; set; }
        public string? GrnDate { get; set; }
        public int GrnSno { get; set; }
        public string? GrnValue { get; set; }
        public int PoNo { get; set; }
        public string? PoDate { get; set; }
        public string? PurchaseValue { get; set; }
        public string? ItemCode { get; set; }
        public string? AcceptedQty { get; set; }
        public string? Uom { get; set; }
        public int PoSno { get; set; }
        public string? ItemName { get; set; }
        public string? BillNo { get; set; }
        public string? BillDate { get; set; }
        public string? BinLocation { get; set; }         
        public string? PjYear { get; set; }
        public string? PjDocId { get; set; }
        public string? PjDocSr { get; set;}
        public int PjDocNo { get; set; }
        public int AssetSourceId { get; set; }   
         public string? CapitalizationDate { get; set; }
    
    }
}