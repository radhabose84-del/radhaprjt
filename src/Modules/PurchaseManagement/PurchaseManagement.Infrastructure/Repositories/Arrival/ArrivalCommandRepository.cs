using System.Data;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.Arrival
{
    public sealed class ArrivalCommandRepository : IArrivalCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public ArrivalCommandRepository(
            ApplicationDbContext db,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _db = db;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(ArrivalHeader entity, int transactionTypeId, CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, ct);

                try
                {
                    _db.Set<ArrivalHeader>().Add(entity);   // header + details (cascade insert)
                    await _db.SaveChangesAsync(ct);

                    // Persist the StockLedgerRaw rows built by the handler (UnitId + LotNo set here).
                    await PersistStockRowsAsync(entity, ct);

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

        public async Task<int> UpdateAsync(ArrivalHeader entity, CancellationToken ct)
        {
            var existing = await _db.Set<ArrivalHeader>()
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                ?? throw new ExceptionRules("Arrival not found.");

            // ArrivalNumber and UnitId are immutable — never updated here.
            existing.ArrivalDate = entity.ArrivalDate;
            existing.RawMaterialPOId = entity.RawMaterialPOId;
            existing.VehicleNumber = entity.VehicleNumber;
            existing.SupplierId = entity.SupplierId;
            existing.StationId = entity.StationId;
            existing.GodownId = entity.GodownId;
            existing.TransporterId = entity.TransporterId;
            existing.FreightRate = entity.FreightRate;
            existing.InvoiceGstNo = entity.InvoiceGstNo;
            existing.LrNumber = entity.LrNumber;
            existing.ContainerNo = entity.ContainerNo;
            existing.LorryIn = entity.LorryIn;
            existing.LorryOut = entity.LorryOut;
            existing.GrossWeight = entity.GrossWeight;
            existing.TareWeight = entity.TareWeight;
            existing.NetWeight = entity.NetWeight;
            existing.PartyWeight = entity.PartyWeight;
            existing.WeightDifference = entity.WeightDifference;
            existing.MoisturePercentage = entity.MoisturePercentage;
            existing.GstPercentage = entity.GstPercentage;
            existing.QcStatusId = entity.QcStatusId;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            // Replace detail lines
            var oldDetails = await _db.Set<ArrivalDetail>()
                .Where(d => d.ArrivalHeaderId == existing.Id)
                .ToListAsync(ct);
            if (oldDetails.Count > 0)
                _db.Set<ArrivalDetail>().RemoveRange(oldDetails);

            foreach (var detail in entity.ArrivalDetails ?? new List<ArrivalDetail>())
            {
                detail.Id = 0;
                detail.ArrivalHeaderId = existing.Id;
                _db.Set<ArrivalDetail>().Add(detail);
            }

            // Regenerate the lot/bale stock ledger for this arrival (StockLedgerRaw has no soft-delete).
            var oldLedger = await _db.Set<StockLedgerRaw>()
                .Where(s => s.LotNo == existing.Id && s.DocType == "ARV")
                .ToListAsync(ct);
            if (oldLedger.Count > 0)
                _db.Set<StockLedgerRaw>().RemoveRange(oldLedger);

            await _db.SaveChangesAsync(ct);

            entity.Id = existing.Id;
            entity.UnitId = existing.UnitId;
            await PersistStockRowsAsync(entity, ct);

            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _db.Set<ArrivalHeader>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            existing.IsActive = Status.Inactive;

            // Remove the generated stock ledger rows for this arrival (StockLedgerRaw has no soft-delete).
            var ledger = await _db.Set<StockLedgerRaw>()
                .Where(s => s.LotNo == id && s.DocType == "ARV")
                .ToListAsync(ct);
            if (ledger.Count > 0)
                _db.Set<StockLedgerRaw>().RemoveRange(ledger);

            await _db.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Persists the StockLedgerRaw rows the handler attached to <paramref name="header"/>.StockRows,
        /// stamping UnitId (from the header) and LotNo (= header Id) on each row.
        /// </summary>
        private async Task PersistStockRowsAsync(ArrivalHeader header, CancellationToken ct)
        {
            if (header.StockRows is not { Count: > 0 })
                return;

            foreach (var row in header.StockRows)
            {
                row.UnitId = header.UnitId;
                row.LotNo = header.Id;
            }

            await _db.Set<StockLedgerRaw>().AddRangeAsync(header.StockRows, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
