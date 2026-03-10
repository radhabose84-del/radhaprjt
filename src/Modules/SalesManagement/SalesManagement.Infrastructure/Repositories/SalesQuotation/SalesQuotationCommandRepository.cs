using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesQuotation
{
    public class SalesQuotationCommandRepository : ISalesQuotationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesQuotationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(SalesQuotationHeader entity)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate details from header
                    var details = entity.SalesQuotationDetails?.ToList();
                    entity.SalesQuotationDetails = null;

                    // Insert header into SalesQuotationHeader table
                    await _applicationDbContext.SalesQuotationHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert details into SalesQuotationDetail table
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.SalesQuotationHeaderId = entity.Id;
                            await _applicationDbContext.SalesQuotationDetail.AddAsync(detail);
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

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

        public async Task<int> UpdateAsync(SalesQuotationHeader entity)
        {
            var existingEntity = await _applicationDbContext.SalesQuotationHeader
                .Include(h => h.SalesQuotationDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Update header fields in SalesQuotationHeader table
                    existingEntity.CustomerId = entity.CustomerId;
                    existingEntity.QuotationDate = entity.QuotationDate;
                    existingEntity.SalesEnquiryId = entity.SalesEnquiryId;
                    existingEntity.ContactPersonId = entity.ContactPersonId;
                    existingEntity.ValidityDate = entity.ValidityDate;
                    existingEntity.PaymentTermId = entity.PaymentTermId;
                    existingEntity.Remarks = entity.Remarks;
                    existingEntity.DeliveryTermId = entity.DeliveryTermId;
                    existingEntity.FreightCharges = entity.FreightCharges;
                    existingEntity.OtherCharges = entity.OtherCharges;
                    existingEntity.TotalBasicAmount = entity.TotalBasicAmount;
                    existingEntity.TotalDiscount = entity.TotalDiscount;
                    existingEntity.NetTaxableAmount = entity.NetTaxableAmount;
                    existingEntity.TotalTax = entity.TotalTax;
                    existingEntity.GrandTotal = entity.GrandTotal;
                    existingEntity.IsActive = entity.IsActive;

                    // Remove existing details from SalesQuotationDetail table
                    if (existingEntity.SalesQuotationDetails != null && existingEntity.SalesQuotationDetails.Any())
                    {
                        _applicationDbContext.SalesQuotationDetail.RemoveRange(existingEntity.SalesQuotationDetails);
                    }

                    // Insert new details into SalesQuotationDetail table
                    if (entity.SalesQuotationDetails != null && entity.SalesQuotationDetails.Any())
                    {
                        foreach (var detail in entity.SalesQuotationDetails)
                        {
                            detail.SalesQuotationHeaderId = existingEntity.Id;
                            await _applicationDbContext.SalesQuotationDetail.AddAsync(detail);
                        }
                    }

                    _applicationDbContext.SalesQuotationHeader.Update(existingEntity);
                    await _applicationDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existingEntity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesQuotationHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesQuotationHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
