using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using System.Text;

namespace Core.Application.Common.Interfaces.ICompany
{
    public interface ICompanyQueryRepository
    {
        Task<(List<Company>,int)> GetAllCompaniesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Company> GetByIdAsync(int id);
        Task<List<Company>> GetCompany(int userId,string searchPattern);
        Task<Company?> GetByCompanynameAsync(string name,int? id = null);
        Task<bool> CompanyExistsAsync(string companyName);
        Task<bool> PanNumberExistsAsync(string panNumber);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<bool> FKColumnExistValidation(int companyId);
        Task<List<Company>> GetCompany_SuperAdmin(string searchPattern);
        Task<bool> IsCompanyUsedByAnyUserAsync(int companyId);

       
    }
}