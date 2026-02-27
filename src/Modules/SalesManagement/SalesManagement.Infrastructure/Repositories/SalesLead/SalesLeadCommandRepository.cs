using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesLead
{
    public class SalesLeadCommandRepository : ISalesLeadCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesLeadCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesLead entity)
        {
            await _applicationDbContext.SalesLead.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesLead entity)
        {
            var existingEntity = await _applicationDbContext.SalesLead
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.PartyId = entity.PartyId;
            existingEntity.ProspectCompanyName = entity.ProspectCompanyName;
            existingEntity.CityId = entity.CityId;
            existingEntity.ContactName = entity.ContactName;
            existingEntity.MobileNumber = entity.MobileNumber;
            existingEntity.EmailId = entity.EmailId;
            existingEntity.ContactId = entity.ContactId;
            existingEntity.ItemId = entity.ItemId;
            existingEntity.RequirementQty = entity.RequirementQty;
            existingEntity.ExpectedDate = entity.ExpectedDate;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.LeadSourceId = entity.LeadSourceId;
            existingEntity.MarketingPersonId = entity.MarketingPersonId;
            existingEntity.InteractionDate = entity.InteractionDate;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.SalesLead.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesLead
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesLead.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
