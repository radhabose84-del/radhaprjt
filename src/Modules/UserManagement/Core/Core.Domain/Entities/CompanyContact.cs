using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class CompanyContact
    {
        public int Id { get; set; }
        
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Remarks { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}