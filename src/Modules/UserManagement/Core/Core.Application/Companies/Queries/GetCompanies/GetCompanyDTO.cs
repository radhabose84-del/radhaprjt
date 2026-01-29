using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Companies.Queries.GetCompanies
{
    public class GetCompanyDTO
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? LegalName { get; set; }
        public string? GstNumber { get; set; }
        public string? TIN { get; set; }
        public string? TAN { get; set; }
        public string? CSTNo { get; set; }
        public int YearOfEstablishment { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? LogoBase64 { get; set; }
        public int EntityId { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }       
       public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public string? PanNumber { get; set; }

        //  public CompanyAddressDTO CompanyAddress { get; set; }
        // public CompanyContactDTO CompanyContact { get; set; }
    }
}