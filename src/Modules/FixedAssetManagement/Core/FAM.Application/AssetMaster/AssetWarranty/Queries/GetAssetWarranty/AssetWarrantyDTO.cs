using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty
{
    public class AssetWarrantyDTO
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public DateOnly? StartDate { get; set; } 
        public DateOnly? EndDate { get; set; } 
        public int? Period { get; set; } 
        public int? WarrantyType { get; set; } 
        public string? WarrantyProvider { get; set; }         
        public string? Description { get; set; }    
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }               
        
        public int ServiceCountryId { get; set; }        
        public int ServiceStateId { get; set; }        
        public int ServiceCityId { get; set; }        
        public string? ServiceAddressLine1 { get; set; }        
        public string? ServiceAddressLine2 { get; set; }        
        public string? ServicePinCode { get; set; }        
        public string? ServiceContactPerson { get; set; }        
        public string? ServiceMobileNumber { get; set; }        
        public string? ServiceEmail { get; set; }   
        public string? ServiceClaimProcessDescription { get; set; } 
        public DateOnly? ServiceLastClaimDate { get; set; } 
        public int? ServiceClaimStatus { get; set; } 
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset?  CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset?  ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; } 
        public string? WarrantyTypeDesc { get; set; } 
        public string? WarrantyClaimStatus { get; set; } 
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public string? Document { get; set; }
        public string? DocumentBase64 { get; set; }
    }
}
