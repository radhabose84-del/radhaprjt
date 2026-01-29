using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.Mappings;
using Core.Domain.Entities;

namespace Core.Application.Companies.Queries.GetCompanies
{
    public class CompanyContactDTO 
    {
        // public int CompanyId { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Remarks { get; set; }


    }
}