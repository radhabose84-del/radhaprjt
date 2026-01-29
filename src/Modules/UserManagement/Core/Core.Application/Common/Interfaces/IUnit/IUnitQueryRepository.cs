using Core.Application.Units.Queries.GetUnits;
using Core.Domain.Entities;



namespace Core.Application.Common.Interfaces.IUnit
{
    public interface IUnitQueryRepository
    {
        Task<(List<Unit>, int)> GetAllUnitsAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<GetUnitsByIdDto> GetByIdAsync(int Id);
        //Task<GetUnitsByIdDto> GetByIdCheckAsync(int Id);   
        Task<List<Unit>> GetUnit(string searchPattern, int userId, int CompanyId);
        Task<List<Unit>> GetUnitByUserId(int userId, int CompanyId);
        Task<bool> FKColumnExistValidation(int Id);
        Task<List<Unit>> GetUnit_SuperAdmin(string searchPattern);
        Task<bool> IsUnitUsedByAnyUserAsync(int unitId);
         
    }
    
   
}