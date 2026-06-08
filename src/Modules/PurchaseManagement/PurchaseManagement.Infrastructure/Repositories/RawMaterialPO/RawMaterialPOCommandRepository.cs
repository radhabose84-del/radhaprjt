using System.Data;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.RawMaterialPO
{
    public sealed class RawMaterialPOCommandRepository : IRawMaterialPOCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public RawMaterialPOCommandRepository(
            ApplicationDbContext db,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _db = db;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(RawMaterialPOHeader entity, int transactionTypeId, CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, ct);

                try
                {
                    _db.Set<RawMaterialPOHeader>().Add(entity);   // header + details (cascade insert)
                    await _db.SaveChangesAsync(ct);

                    var dbConnection = _db.Database.GetDbConnection();
                    var dbTransaction = tx.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return entity.Id;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> UpdateAsync(RawMaterialPOHeader entity, CancellationToken ct)
        {
            var existing = await _db.Set<RawMaterialPOHeader>()
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                ?? throw new ExceptionRules("Raw Material PO not found.");

            // PONumber, OcrId and UnitId are immutable — never updated here.
            existing.PODate = entity.PODate;
            existing.ProcurementDocumentTypeId = entity.ProcurementDocumentTypeId;
            existing.StatusId = entity.StatusId;
            existing.Remarks = entity.Remarks;
            existing.TaxableTotal = entity.TaxableTotal;
            existing.TotalGstAmount = entity.TotalGstAmount;
            existing.NetTotal = entity.NetTotal;
            existing.IsActive = entity.IsActive;

            // Additional cotton details
            existing.CropYear = entity.CropYear;
            existing.ArrivalType = entity.ArrivalType;
            existing.PassingDate = entity.PassingDate;
            existing.CreditDays = entity.CreditDays;
            existing.CottonApprovedBy = entity.CottonApprovedBy;
            existing.CottonApprovedOn = entity.CottonApprovedOn;
            existing.DocumentPath = entity.DocumentPath;

            // Replace detail lines
            var oldDetails = await _db.Set<RawMaterialPODetail>()
                .Where(d => d.POHeaderId == existing.Id)
                .ToListAsync(ct);
            if (oldDetails.Count > 0)
                _db.Set<RawMaterialPODetail>().RemoveRange(oldDetails);

            foreach (var detail in entity.RawMaterialPODetails ?? new List<RawMaterialPODetail>())
            {
                detail.Id = 0;
                detail.POHeaderId = existing.Id;
                _db.Set<RawMaterialPODetail>().Add(detail);
            }

            await _db.SaveChangesAsync(ct);
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _db.Set<RawMaterialPOHeader>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            existing.IsActive = Status.Inactive;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ClearDocumentPathByFileNameAsync(string fileName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var existing = await _db.Set<RawMaterialPOHeader>()
                .FirstOrDefaultAsync(x => x.DocumentPath == fileName && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            // Audit fields (ModifiedBy/Date/IP) are auto-populated by ApplicationDbContext.SaveChangesAsync.
            existing.DocumentPath = null;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
