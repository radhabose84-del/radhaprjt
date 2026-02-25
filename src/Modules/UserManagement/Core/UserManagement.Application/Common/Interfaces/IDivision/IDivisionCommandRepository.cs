using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IDivision
{
    public interface IDivisionCommandRepository
    {  
        Task<Division> CreateAsync(Division division);     
        Task<bool> UpdateAsync(Division division);
        Task<bool> DeleteAsync(int id,Division division);        
    }
}