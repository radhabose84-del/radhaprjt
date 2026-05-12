using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesAgreement
{
    public class SalesAgreementCommandRepository : ISalesAgreementCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public SalesAgreementCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(SalesAgreementHeader entity, int transactionTypeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate detail lines from header to insert them after the header Id is known
                    var details = entity.SalesAgreementDetails?.ToList();
                    entity.SalesAgreementDetails = null;

                    await _applicationDbContext.SalesAgreementHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.SalesAgreementHeaderId = entity.Id;
                            // ReleasedQty defaults to 0 — set by AutoMapper profile, enforced by DB default
                            await _applicationDbContext.SalesAgreementDetail.AddAsync(detail);
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

                    // Increment DocNo atomically within the same SQL transaction
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

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

        public async Task<int> UpdateAsync(SalesAgreementHeader entity)
        {
            var existing = await _applicationDbContext.SalesAgreementHeader
                .Include(h => h.SalesAgreementDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Update mutable header fields (AgreementNo is immutable)
                    existing.StatusId = entity.StatusId;
                    existing.ValidFrom = entity.ValidFrom;
                    existing.ValidTo = entity.ValidTo;
                    existing.CustomerId = entity.CustomerId;
                    existing.SalesGroupId = entity.SalesGroupId;
                    existing.PaymentTermsId = entity.PaymentTermsId;
                    existing.Remarks = entity.Remarks;
                    existing.IsActive = entity.IsActive;

                    // Diff details: match by Id; new lines insert; rows omitted from payload are removed
                    var existingDetails = existing.SalesAgreementDetails?.ToList() ?? new List<SalesAgreementDetail>();
                    var incomingDetails = entity.SalesAgreementDetails?.ToList() ?? new List<SalesAgreementDetail>();

                    var incomingIds = incomingDetails.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();

                    // Hard-delete rows that the client no longer sends (header soft-delete still hides them via header.IsDeleted)
                    var toRemove = existingDetails.Where(d => !incomingIds.Contains(d.Id)).ToList();
                    if (toRemove.Count > 0)
                        _applicationDbContext.SalesAgreementDetail.RemoveRange(toRemove);

                    foreach (var incoming in incomingDetails)
                    {
                        if (incoming.Id > 0)
                        {
                            var match = existingDetails.FirstOrDefault(d => d.Id == incoming.Id);
                            if (match != null)
                            {
                                match.ItemId = incoming.ItemId;
                                match.VariantId = incoming.VariantId;
                                match.AgreedRate = incoming.AgreedRate;
                                match.TotalQty = incoming.TotalQty;
                                // ReleasedQty is system-maintained — preserved (not overwritten)
                            }
                        }
                        else
                        {
                            incoming.SalesAgreementHeaderId = existing.Id;
                            incoming.ReleasedQty = 0m;
                            await _applicationDbContext.SalesAgreementDetail.AddAsync(incoming);
                        }
                    }

                    _applicationDbContext.SalesAgreementHeader.Update(existing);
                    await _applicationDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existing.Id;
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
            var existing = await _applicationDbContext.SalesAgreementHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesAgreementHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
