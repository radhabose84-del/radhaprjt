using FAM.Domain.Common;

namespace FAM.Domain.Entities.AssetMaster
{
    public class  AssetWarranties : BaseEntity
    {
        public int AssetId { get; set; } 
        public AssetMasterGenerals AssetMasterId { get; set; } = null!;
        public DateOnly? StartDate { get; set; } 
        public DateOnly? EndDate { get; set; } 
        public int? Period { get; set; }         
        public int? WarrantyType { get; set; } 
        public MiscMaster MiscWarrantyTypes { get; set; } = null!;    
        public string? WarrantyProvider { get; set; } 
        public string? Description { get; set; }    
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }       
        public string? Document { get; set; }
        //Service Center Deatils
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
        public MiscMaster MiscClaimStatus { get; set; } = null!;   

    }
}