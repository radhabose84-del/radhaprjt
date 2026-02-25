using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Data;

namespace PartyManagement.Infrastructure.Repositories.PartyGroup
{
    public class PartyGroupCommandRepository : IPartyGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public PartyGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(PartyManagement.Domain.Entities.PartyGroup partyGroup)
        {
            // Add the PartyGroup to the DbContext
            await _applicationDbContext.PartyGroup.AddAsync(partyGroup);

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();

            // Return the ID of the created PartyGroup
            return partyGroup.Id;
        }

        public async Task<bool> DeleteAsync(int Id, PartyManagement.Domain.Entities.PartyGroup partyGroup)
        {
            // Fetch the PartyGroup to delete from the database
            var partyGroupToDelete = await _applicationDbContext.PartyGroup.FirstOrDefaultAsync(u => u.Id == Id);

            // If the PartyGroup does not exist
            if (partyGroupToDelete is null)
            {
                return false; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            partyGroupToDelete.IsDeleted = partyGroup.IsDeleted;

            // Save changes to the database 
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsAsync(string partyGroupName, int groupTypeId)
        {
            return await _applicationDbContext.PartyGroup
                         .AnyAsync(pg => pg.PartyGroupName == partyGroupName && pg.GroupTypeId == groupTypeId);
        }

        public async Task<bool> ExistsUpdateAsync(string partyGroupName, int groupTypeId, int? currentId = null)
        {
            return await _applicationDbContext.PartyGroup
                        .AnyAsync(pg =>
                        pg.PartyGroupName == partyGroupName &&
                        pg.GroupTypeId == groupTypeId &&
                        pg.Id != currentId);
        }

        public async Task<bool> UpdateAsync(int Id, PartyManagement.Domain.Entities.PartyGroup partyGroup)
        {
            var existingpartyGroup = await _applicationDbContext.PartyGroup.FirstOrDefaultAsync(u => u.Id == Id);

            // If the PartyGroup does not exist
            if (existingpartyGroup is null)
            {
                return false; //indicate failure
            }

            // Update the existing PartyGroup properties
            existingpartyGroup.PartyGroupName = partyGroup.PartyGroupName;
            existingpartyGroup.ParentPartyGroupId = partyGroup.ParentPartyGroupId;
            existingpartyGroup.Description = partyGroup.Description;
            existingpartyGroup.Glcode= partyGroup.Glcode;
            existingpartyGroup.GlCategoryId = partyGroup.GlCategoryId;
            existingpartyGroup.IsGroup = partyGroup.IsGroup;
            existingpartyGroup.IsActive = partyGroup.IsActive;

            // Mark the entity as modified
            _applicationDbContext.PartyGroup.Update(existingpartyGroup);

            // Save changes to the database
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }
        
        public async Task<int?> GetGroupTypeIdByIdAsync(int id)
        {
            return await _applicationDbContext.PartyGroup
                .Where(pg => pg.Id == id)
                .Select(pg => (int?)pg.GroupTypeId)
                .FirstOrDefaultAsync();
        }
    }
}