
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Domain.Entities
{
    public class AssetMasterGenerals : BaseEntity
    {
        public int CompanyId { get; set; } 
        public int UnitId { get; set; } 
        public string? AssetCode { get; set; }        
        public string? AssetName { get; set; }        
        // Foreign Key
        public int AssetGroupId { get; set; }
        public AssetGroup AssetGroup { get; set; } = null!;     
        public int? AssetSubGroupId { get; set; }
        public AssetSubGroup? AssetSubGroup { get; set; } = null!;       
        public int AssetCategoryId { get; set; }
        public AssetCategories AssetCategories { get; set; } = null!;
        public int AssetSubCategoryId { get; set; }
        public AssetSubCategories AssetSubCategories { get; set; } = null!;
        public int? AssetParentId { get; set; }
        public AssetMasterGenerals AssetParent { get; set; } = null!;
        public ICollection<AssetMasterGenerals>? AssetChildren  { get; set; }        
        public int? AssetType { get; set; }     
        public MiscMaster? AssetMiscType { get; set; } = null!;       
        //End Foreign Key
        public string? MachineCode { get; set; }   
        public int? Quantity { get; set; }
        public int? UOMId { get; set; }
        public UOM UomMaster { get; set; } = null!;
        public string? AssetDescription { get; set; }
        public int? WorkingStatus { get; set; }
        public MiscMaster? AssetWorkType { get; set; } = null!;   
        public string? AssetImage { get; set; }
        public byte ISDepreciated { get; set; }
        public byte IsTangible { get; set; }    
        public string? AssetDocument { get; set; }
        public DateTimeOffset? PutToUseDate { get; set; }
        public ICollection<AssetPurchaseDetails>? AssetPurchase { get; set; }     
		public ICollection<AssetSpecifications>? AssetSpecification { get; set; }
		public ICollection<AssetWarranties>? AssetWarranty { get; set; }
        public ICollection<AssetAdditionalCost>? AssetAdditionalCost { get; set; }
        public AssetLocation? AssetLocation { get; set; }      
 		public ICollection<AssetAmc>? AssetAmc { get; set; }    
		public  ICollection<AssetInsurance>? AssetInsurance { get; set; }
        public AssetDisposal? AssetDisposalMaster { get; set; } 
		public ICollection<DepreciationDetails>? DepreciationDetails { get; set; }
        public ICollection<AssetTransferIssueDtl>? AssetTransferIssueMaster { get; set; } 
        public ICollection<AssetTransferReceiptDtl>? AssetTransferReceiptMaster { get; set; }

    }
}