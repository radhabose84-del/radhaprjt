using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.ICompany
{
    public interface ICompanyCommandRepository
    {
        Task<int> CreateAsync(Company company);
        Task<bool> UpdateAsync(int id,Company company);
        Task<bool> DeleteAsync(int id,Company company);      
    }
}