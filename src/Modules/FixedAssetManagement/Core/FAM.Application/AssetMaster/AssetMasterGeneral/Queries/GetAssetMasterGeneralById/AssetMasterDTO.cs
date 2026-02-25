namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetMasterDTO
    {
        public string? AssetName { get; set; }
        public string? AssetCode { get; set; }
        public int Quantity { get; set; }
        public string? UOMName { get; set; }
        public string? GroupName { get; set; }
        public string? SubGroupName { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? ParentName { get; set; }        
        public string? AssetGroupId { get; set; }
        public int? AssetSubGroupId { get; set; }  
        public string? AssetImage { get; set; }            
        public int AssetCategoryId { get; set; }        
        public int AssetSubCategoryId { get; set; }        
        public int? AssetParentId { get; set; }        
        public int? AssetType { get; set; }                
        public int? UOMId { get; set; }
        public int? WorkingStatus { get; set; }
        public string? AssetImageName { get; set; }     
        public string? AssetDocument { get; set; }     
        public string? AssetDocumentName { get; set; }             
        public DateTimeOffset? PutToUseDate { get; set; }
        public AssetParentDTO? AssetParent { get; set; }
        public AssetLocationDTO? AssetLocation { get; set; }
        public IList<AssetPurchaseDetailDTO>? AssetPurchaseDetails { get; set; }
        public IList<AssetSpecDTO>? AssetSpecification { get; set; }
        public IList<AssetWarrantyDTOById>? AssetWarranty { get; set; }
        public IList<AssetAMCDTOById>? AssetAmc { get; set; }
        public AssetDisposalByIdDTO? AssetDisposal { get; set; }
        public IList<AssetInsuranceByIdDTO>? AssetInsurance { get; set; }
        public IList<AssetAdditionalCostDto>? AssetAdditionalCost { get; set; }
    }
}