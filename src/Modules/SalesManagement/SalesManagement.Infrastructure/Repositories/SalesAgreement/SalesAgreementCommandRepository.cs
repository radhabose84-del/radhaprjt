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

        public async Task<bool> UpdateAgentPOAttachmentAsync(int id, string fileName, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesAgreementHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.AgentPOAttachment = fileName;
            _applicationDbContext.SalesAgreementHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CancelAsync(int id, int cancelledStatusId, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesAgreementHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.StatusId = cancelledStatusId;
            _applicationDbContext.SalesAgreementHeader.Update(existing);
            // ModifiedBy / ModifiedDate / ModifiedByName / ModifiedIP are auto-populated
            // by ApplicationDbContext.SaveChangesAsync() for any BaseEntity in the Modified state.
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
