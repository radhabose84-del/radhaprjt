using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICompanyLookup
    {
      Task<List<CompanyLookupDto>> GetAllCompanyAsync();
    }
}