using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces.IEntity;
using static Core.Domain.Enums.Common.Enums;


namespace UserManagement.Infrastructure.Repositories.Entities
{
    public class EntityCommandRepository : IEntityCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public EntityCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
    


    public async Task<int> CreateAsync(Entity entity)
    {
    // Add the entity to the DbContext
        await _applicationDbContext.Entity.AddAsync(entity);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created entity
        return entity.Id;
    
  
}

       
    public async Task<int> UpdateAsync(int id, Entity entity)
    {   
    
        var existingEntity = await _applicationDbContext.Entity.FirstOrDefaultAsync(u => u.Id == id);

        // If the entity does not exist, throw a CustomException
        if (existingEntity == null)
        {
            return -1; //indicate failure
        }

        // Update the existing entity's properties
        existingEntity.EntityName = entity.EntityName;
        existingEntity.EntityDescription = entity.EntityDescription;
        existingEntity.Address = entity.Address;
        existingEntity.Phone = entity.Phone;
        existingEntity.Email = entity.Email;
        existingEntity.IsActive = entity.IsActive;

        // Mark the entity as modified
        _applicationDbContext.Entity.Update(existingEntity);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
    
}

public async Task<int> DeleteEntityAsync(int id, Entity entity)
{
    
    //     // Fetch the entity to delete from the database
        var entityToDelete = await _applicationDbContext.Entity.FirstOrDefaultAsync(e => e.Id == id);

        // If the entity does not exist, throw a CustomException
        if (entityToDelete is null)
        {
            return -1; //indicate failure
        }

        // Update the IsActive status to indicate deletion (or soft delete)
        entityToDelete.IsDeleted = entity.IsDeleted;


        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
   
}

    public async Task<bool> ExistsByCodeAsync(string entity)
    {
        return await _applicationDbContext.Entity.AnyAsync(c => c.EntityName == entity);
    }

    public async Task<bool> ExistsByNameupdateAsync(string name, int id)
    {
        return await _applicationDbContext.Entity.AnyAsync(c => c.EntityName == name && c.Id != id);
    }

      
    }
}