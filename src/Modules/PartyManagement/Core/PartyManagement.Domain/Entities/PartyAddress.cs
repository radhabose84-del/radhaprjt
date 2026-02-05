using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Domain.Entities
{
    public class PartyAddress
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public PartyMaster PartyAddressId { get; set; } = null!;
        public string? AddressType { get; set; } // Billing, Shipping, etc.
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string? PostalCode { get; set; }
        public int CountryId { get; set; }
   
 
    }
}