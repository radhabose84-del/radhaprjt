using Core.Domain.Entities;



namespace Core.Application.Common.Interfaces.IUnit
{
    public interface IUnitCommandRepository
    {      
       Task<int> CreateUnitAsync(Unit unit);
       Task<int> UpdateUnitAsync(int Id, Unit unit);
       Task<int> DeleteUnitAsync(int Id, Unit unit);
       Task<bool> ExistsByCodeAsync(string code); 
       Task<bool> ExistsByNameupdateAsync(string name,int id );
       
    }
    

   
}