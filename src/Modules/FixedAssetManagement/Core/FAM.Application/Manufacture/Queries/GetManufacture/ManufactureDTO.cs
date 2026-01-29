using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Manufacture.Queries.GetManufacture
{
    public class ManufactureDTO : IMapFrom<Manufactures>
    {
        public int Id { get; set; }
        public string? Code { get; set; }        
        public string? ManufactureName { get; set; }                
        public string? ManufactureType { get; set; }
        public int CountryId { get; set; }        
        public int StateId { get; set; }        
        public int CityId { get; set; }        
        public string? AddressLine1 { get; set; }        
        public string? AddressLine2 { get; set; }        
        public string? PinCode { get; set; }        
        public string? PersonName { get; set; }        
        public string? PhoneNumber { get; set; }        
        public string? Email { get; set; }  
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset?  CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset?  ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }   
        public string? CountryName { get; set; }        
        public string? StateName { get; set; }        
        public string? CityName { get; set; }
        public IsDelete IsDeleted { get; set; }
        public string? ManufactureTypeDescription { get; set; }
        
    }
}