using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Infrastructure.Data;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Repositories.GateInward
{
    public class GateInwardCommandRepository : IGateInwardCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public GateInwardCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(GateInwardHdr entity, int transactionTypeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _applicationDbContext.GateInwardHdr.AddAsync(entity);
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

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GateInwardHdr
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.GateInwardHdr.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<string?> ClearAttachmentAsync(int gateInwardHdrId, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GateInwardHdr
                .FirstOrDefaultAsync(x => x.Id == gateInwardHdrId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null || string.IsNullOrWhiteSpace(existing.AttachmentFilePath))
                return null;

            var oldPath = existing.AttachmentFilePath;

            existing.AttachmentFileName = null;
            existing.AttachmentOriginalFileName = null;
            existing.AttachmentFilePath = null;
            existing.AttachmentFileType = null;
            existing.AttachmentFileSize = null;

            _applicationDbContext.GateInwardHdr.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return oldPath;
        }
    }
}
