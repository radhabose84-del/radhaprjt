using FAM.Domain.Entities;

namespace FAM.Application.Common.Interfaces.IManufacture
{
    public interface IManufactureCommandRepository
    {
        Task<Manufactures> CreateAsync(Manufactures manufacture);
        Task<bool> UpdateAsync(Manufactures manufacture);
        Task<int> DeleteAsync(int Id, Manufactures manufacture);
        Task<bool> ExistsByCodeAsync(string code, int? Id = null); 
        Task<bool> ExistsByNameAsync(string name, int? id = null);
       
    }    
}