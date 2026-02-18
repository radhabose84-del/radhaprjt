    namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems
    {
        public class ItemDto
        {
            // ItemMaster (base)
            public int Id { get; set; }
            public int UnitId { get; set; }
            public string? ItemCode { get; set; }
            public string? ItemName { get; set; }
            public int? HSNId { get; set; }
            public int? ItemGroupId { get; set; }
            public int? ItemCategoryId { get; set; }
            public int? StockUomId { get; set; }
            public int? ItemClassificationId { get; set; }
            public string? Description { get; set; }
            public DateOnly? ValidFrom { get; set; }
            public int? XPlantMaterialStatusId { get; set; }
            public bool IsStockItem { get; set; }
            public bool IsCapitalItem { get; set; }
            public bool MaintainStock { get; set; }
            public bool HasVariants { get; set; }
            public int? ParentItemId { get; set; }
            public string? ItemImage { get; set; }
            public int IsActive { get; set; }
            public int IssueRuleId { get; set; }
            public bool IsOnSpot { get; set; } = false;
            // Tabs
            public ItemPurchaseDto? Purchase { get; set; }
            public ItemInventoryDto? Inventory { get; set; }
            public ItemQualityDto? Quality { get; set; }
            // Collections
            public List<ItemSupplierDto> Suppliers { get; set; } = new();
            public List<ItemManufactureDto> Manufacture { get; set; } = new();
            public List<ItemUomDto> Uoms { get; set; } = new();
            public List<VariantAttributeDto> VariantAttributes { get; set; } = new();
            public List<VariantValueDto> VariantValues { get; set; } = new();   
        }
    public class ItemDetailsDto : ItemDto
    {
        public new int Id { get; set; }
        public string? HSNCode { get; set; }
        public string? ItemGroupName { get; set; }
        public string? ItemCategoryName { get; set; }
        public string? StockUOM { get; set; }
        public string? ItemClassification { get; set; }
        public string? XPlantMaterialStatus { get; set; }
        public string? ParentItemName { get; set; }
        public string? UnitName { get; set; }
        public decimal GSTPercentage { get; set; }
        public string? ItemImageUrl { get; set; }
        public string? IssueRule { get; set; }            
    }
    public class ItemPurchaseDto
    {
        public int Id { get; set; }
        public int? PurchaseUomId { get; set; }
        public int? LeadTimeDays { get; set; }
        public int? SafetyStock { get; set; }
        public int? GrProcessingTimeDays { get; set; }
        public decimal? PurchaseRate { get; set; }
        public bool AutomaticPo { get; set; }
        public int? OriginCountryId { get; set; }
        public string? TariffNumber { get; set; }
        public string? PurchaseUOM { get; set; }
        public int? SourceOfItem { get; set; }
        }
        public class ItemInventoryDto
        {
            public int Id { get; set; }
            public decimal? Weight { get; set; }
            public int? WeightUomId { get; set; }
            public int? DefaultMaterialRequestTypeId { get; set; }
            public int? ValuationMethodId { get; set; }
            public int? ShelfLife { get; set; }
            public decimal? UpperTolerance { get; set; }
            public decimal? LowerTolerance { get; set; }
            public string? BatchNumberSeries { get; set; }
            public string? SerialNumberSeries { get; set; }
            public int? ReorderLevel { get; set; }
            public int? ReorderQty { get; set; }
            public int? RequestTypeId { get; set; }
            public bool AllowNegativeStock { get; set; }
            public bool BatchManagement { get; set; }
            public bool ApplyBatchNumber { get; set; }
            public string? DefaultMaterialRequestType { get; set; }
            public string? ValuationMethod { get; set; }
            public string? RequestType { get; set; }
            public string? InventoryUOM { get; set; }
        }
        public class ItemQualityDto
        {
            public int Id { get; set; }
            public int? InspectionTemplateId { get; set; }
            public int? CertificateTypeId { get; set; }
            public int? InspLotProcessingTime { get; set; }
            public bool InspectionRequired { get; set; }
            public bool QualityInspectionFree { get; set; }
            public bool IsCertificateRequiredFromSupplier { get; set; }
            public string? CertificateType { get; set; }
        }
        public class ItemSupplierDto
        {
            public int SupplierId { get; set; }
            public int UnitId { get; set; }
            public string? SupplierPartNo { get; set; }
            public int? LeadTime { get; set; }
            public int? MOQ { get; set; }
            public int? MOQUomId { get; set; }
            public decimal? PackageValue { get; set; }
            public int? PackageUomId { get; set; }
            public bool? DefaultSupplier { get; set; }
        }
        public class ItemManufactureDto
        {
            public int UnitId { get; set; }
            public int ManufacturingTypeId { get; set; }
            public string? ManufacturingType { get; set; }
        }
        public class VariantAttributeDto
        {
            public int Id { get; set; }
            public int AttributeId { get; set; }
            public int VariantBasedOn { get; set; }
            public int? AttributeGroupId { get; set; }
            public int Order { get; set; }      
            public string? AttributeName  { get; set; }  
        }
        
        public class VariantValueDto
        {        
            public int? VariantAttributeId { get; set; }
            public string OptionValue { get; set; } = null!;
            public int? Combo { get; set; }             
        }
    
        public class ItemUomDto
        {
            public int? ConversionUOMId { get; set; }
            public decimal? ConversionRate { get; set; }
            public string? ConversionUOM { get; set; }
        }
    }
