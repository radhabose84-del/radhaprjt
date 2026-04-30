using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesQuotation
{
    public class SalesQuotationCommandRepository : ISalesQuotationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public SalesQuotationCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(SalesQuotationHeader entity, int transactionTypeId)
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

                    // Increment DocNo via lookup — same connection + transaction
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    await _applicationDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"SalesQuotation creation failed: {ex.Message}", ex);
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

        public async Task<SalesQuotationWorkFlowDto> GetByIdSalesQuotationWorkFlowAsync(int id)
        {
            var entity = await _applicationDbContext.SalesQuotationHeader
                .Where(x => x.Id == id)
                .Select(x => new SalesQuotationWorkFlowDto
                {
                    Id = x.Id,
                    QuotationNo = x.QuotationNo,
                    StatusId = x.StatusId,
                    StatusName = x.StatusMisc != null ? x.StatusMisc.Description : null
                })
                .FirstOrDefaultAsync();

            return entity!;
        }

        public async Task<SalesQuotationHeader?> GetByIdEntityAsync(int id)
        {
            return await _applicationDbContext.SalesQuotationHeader
                .Include(h => h.SalesQuotationDetails)
                .FirstOrDefaultAsync(h => h.Id == id && h.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task FinalizeQuotationStatusAsync(SalesQuotationHeader entity)
        {
            _applicationDbContext.SalesQuotationHeader.Update(entity);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
