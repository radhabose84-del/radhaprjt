using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using System.Text;

namespace Core.Application.Common.Interfaces.ICompany
{
    public interface ICompanyCommandRepository
    {
        Task<int> CreateAsync(Company company);
        Task<bool> UpdateAsync(int id,Company company);
        Task<bool> DeleteAsync(int id,Company company);      
    }
}