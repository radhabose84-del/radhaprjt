namespace PurchaseManagement.Application.PriceMaster.Dtos
{
    public sealed class PriceMasterDetailUpsertDto
    {
        public int? Id { get; set; }
        public decimal ScaleQtyFrom { get; set; }
        public decimal? ScaleQtyTo { get; set; }
        public decimal UnitPrice { get; set; }  
        public int CurrencyId { get; set; }   
        public string? CurrencyName { get; set; }   
        public int IsActive { get; set; } 
    }

    public sealed class PriceMasterCreateDto
    {        
        public int ItemId { get; set; }
        public int VendorId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; } 
        public int UomId { get; set; }
        public List<PriceMasterDetailUpsertDto> Details { get; set; } = new();
    }

    public sealed class PriceMasterUpdateDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int VendorId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }                    
        public int UomId { get; set; }
        public int IsActive { get; set; }
        public List<PriceMasterDetailUpsertDto> Details { get; set; } = new();
    }
    
    public sealed class PriceMasterGetAllDto
    {
        public int Id { get; set; }                  
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public string? VendorCode { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }        
        public int StatusId { get; set; }      
        public string? StatusName { get; set; } 
        public int SourceFromId { get; set; }
        public string? SourceFrom { get; set; }
        public int SourceDetailId { get; set; }
        public int UomId { get; set; }
        public string? UOM { get; set; }
        public int IsActive { get; set; } 
        public List<PriceMasterDetailUpsertDto> Details { get; set; } = new();
    }
}
