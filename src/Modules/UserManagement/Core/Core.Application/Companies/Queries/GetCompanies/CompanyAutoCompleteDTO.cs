using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Companies.Queries.GetCompanies
{
    public class CompanyAutoCompleteDTO
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
    }
}