using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.ICompanySettings
{
    public interface ICompanyCommandSettings
    {
        Task<int> CreateAsync(UserManagement.Domain.Entities.CompanySettings companySettings);
        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.CompanySettings companySettings);
        Task<bool> DeleteAsync(int id);
    }
}