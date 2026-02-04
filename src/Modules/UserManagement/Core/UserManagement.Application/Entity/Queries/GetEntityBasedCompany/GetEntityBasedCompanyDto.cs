using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Entity.Queries.GetEntityBasedCompany
{
    public class GetEntityBasedCompanyDto
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }
}