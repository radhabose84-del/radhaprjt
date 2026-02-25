namespace UserManagement.Application.Common.Interfaces.IEntity
{
    using Entity = UserManagement.Domain.Entities.Entity;
    public interface IEntityCommandRepository
    {
      
    
      Task<int> CreateAsync(Entity entity);
      Task<int> UpdateAsync(int Id,Entity entity);
      Task<int> DeleteEntityAsync(int Id,Entity entity);
      Task<bool> ExistsByCodeAsync(string entity); // Check if code exists
      Task<bool> ExistsByNameupdateAsync(string name,int id );
      
         



       
    }
}