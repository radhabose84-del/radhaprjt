using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesEnquiry
{
    public class SalesEnquiryCommandRepository : ISalesEnquiryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public SalesEnquiryCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(SalesEnquiryHeader entity, int transactionTypeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate details from header
                    var details = entity.SalesEnquiryDetails?.ToList();
                    entity.SalesEnquiryDetails = null;

                    // Insert header
                    await _applicationDbContext.SalesEnquiryHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert details
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.SalesEnquiryHeaderId = entity.Id;
                            await _applicationDbContext.SalesEnquiryDetail.AddAsync(detail);
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

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

        public async Task<int> UpdateAsync(SalesEnquiryHeader entity)
        {
            var existingEntity = await _applicationDbContext.SalesEnquiryHeader
                .Include(h => h.SalesEnquiryDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.PartyId = entity.PartyId;
            existingEntity.EnquiryDate = entity.EnquiryDate;
            existingEntity.ContactPerson = entity.ContactPerson;
            existingEntity.ExpectedDeliveryDate = entity.ExpectedDeliveryDate;
            existingEntity.PaymentTermId = entity.PaymentTermId;
            existingEntity.SalesLeadId = entity.SalesLeadId;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.IsActive = entity.IsActive;

            // Remove existing details and replace with new ones
            if (existingEntity.SalesEnquiryDetails != null && existingEntity.SalesEnquiryDetails.Any())
            {
                _applicationDbContext.SalesEnquiryDetail.RemoveRange(existingEntity.SalesEnquiryDetails);
            }

            if (entity.SalesEnquiryDetails != null && entity.SalesEnquiryDetails.Any())
            {
                foreach (var detail in entity.SalesEnquiryDetails)
                {
                    detail.SalesEnquiryHeaderId = existingEntity.Id;
                    await _applicationDbContext.SalesEnquiryDetail.AddAsync(detail);
                }
            }

            _applicationDbContext.SalesEnquiryHeader.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesEnquiryHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesEnquiryHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
