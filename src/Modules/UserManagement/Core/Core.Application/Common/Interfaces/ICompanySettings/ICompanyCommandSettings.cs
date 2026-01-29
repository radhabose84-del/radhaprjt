using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.ICompanySettings
{
    public interface ICompanyCommandSettings
    {
        Task<int> CreateAsync(Core.Domain.Entities.CompanySettings companySettings);
        Task<bool> UpdateAsync(int id, Core.Domain.Entities.CompanySettings companySettings);
        Task<bool> DeleteAsync(int id);
    }
}