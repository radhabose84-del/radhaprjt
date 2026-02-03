using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class CountryLookupDto
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
    }
}