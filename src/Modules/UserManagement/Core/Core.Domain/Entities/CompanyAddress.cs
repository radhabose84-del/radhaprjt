using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class CompanyAddress
    {
        public int Id { get; set; }
        
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PinCode { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public string? Phone { get; set; }
        public string? AlternatePhone { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}