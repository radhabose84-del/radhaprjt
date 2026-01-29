using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Domain.Entities
{
    public class Manufactures  : BaseEntity
    {      
        public string? Code { get; set; }        
        public string? ManufactureName { get; set; }                
        public int? ManufactureType { get; set; }
        public MiscMaster ManufactureTypes { get; set; } = null!;    
        public int CountryId { get; set; }        
        public int StateId { get; set; }        
        public int CityId { get; set; }        
        public string? AddressLine1 { get; set; }        
        public string? AddressLine2 { get; set; }        
        public string? PinCode { get; set; }        
        public string? PersonName { get; set; }        
        public string? PhoneNumber { get; set; }        
        public string? Email { get; set; }   
       /*  public ICollection<AssetSpecifications>? AssetSpecification { get; set; }        */        
    }
}