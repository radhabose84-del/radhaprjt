using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty
{
    public class CreateAssetWarrantyCommand : IRequest<AssetWarrantyDTO>
    { 
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
        //Service Center Details
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
    }
}