using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Core.Domain.Common;


namespace Core.Domain.Entities
{
    
    public class Company : BaseEntity
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
        public int EntityId { get; set; }
        public  CompanyAddress? CompanyAddress { get; set; }
        public  CompanyContact? CompanyContact { get; set; }
        public IList<UserCompany>? UserCompanies { get; set; }
        public CompanySettings? CompanySettings { get; set; }
        public IList<Unit>? Units { get; set; }
        public IList<Division>? Divisions { get; set; }
        public string? PanNumber { get; set; }
    }
}