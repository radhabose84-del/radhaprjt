using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesLead
{
    public class SalesLeadCommandRepository : ISalesLeadCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public SalesLeadCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesLead entity, int transactionTypeId, Domain.Entities.SalesContact? newContact = null)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Create contact inside the same transaction so it rolls back if lead creation fails
                    if (newContact != null)
                    {
                        await _applicationDbContext.SalesContact.AddAsync(newContact);
                        await _applicationDbContext.SaveChangesAsync();
                        entity.ContactId = newContact.Id;
                    }

                    await _applicationDbContext.SalesLead.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Increment DocNo via lookup — same connection + transaction
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    await _applicationDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
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
            existingEntity.VariantId = entity.VariantId;
            existingEntity.UomId = entity.UomId;
            existingEntity.RequirementQty = entity.RequirementQty;
            existingEntity.ExpectedDate = entity.ExpectedDate;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.LeadSourceId = entity.LeadSourceId;
            existingEntity.MarketingOfficerId = entity.MarketingOfficerId;
            existingEntity.InteractionDate = entity.InteractionDate;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.SalesLead.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<int> CloseAsync(Domain.Entities.SalesLead entity)
        {
            var existingEntity = await _applicationDbContext.SalesLead
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.ClosureTypeId = entity.ClosureTypeId;
            existingEntity.ClosureReasonId = entity.ClosureReasonId;
            existingEntity.ConvertWonLeadToId = entity.ConvertWonLeadToId;
            existingEntity.ClosureRemarks = entity.ClosureRemarks;
            existingEntity.ClosureDate = entity.ClosureDate;

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
