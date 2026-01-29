using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.Mappings;
using Core.Domain.Entities;

namespace Core.Application.Companies.Queries.GetCompanies
{
    public class CompanyAddressDTO 
    {
        // public int CompanyId { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PinCode { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public string? Phone { get; set; }
        public string? AlternatePhone { get; set; }

        
    }
}