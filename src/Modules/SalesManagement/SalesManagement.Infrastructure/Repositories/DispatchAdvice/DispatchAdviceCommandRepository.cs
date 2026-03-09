using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DispatchAdvice
{
    public class DispatchAdviceCommandRepository : IDispatchAdviceCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DispatchAdviceCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<string> GenerateNextDispatchNoAsync(int unitId, CancellationToken ct = default)
        {
            var prefix = $"DA-{unitId}-";

            var lastDispatch = await _applicationDbContext.DispatchAdviceHeader
                .Where(x => x.DispatchNo != null && x.DispatchNo.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.DispatchNo)
                .FirstOrDefaultAsync(ct);

            int nextSeq = 1;
            if (lastDispatch != null)
            {
                var parts = lastDispatch.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(DispatchAdviceHeader entity, int unitId, int packedStatusId, int reservedStatusId)
        {
            // Separate details from header
            var details = entity.DispatchAdviceDetails?.ToList();
            entity.DispatchAdviceDetails = null;

            // Insert header
            await _applicationDbContext.DispatchAdviceHeader.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();

            // Insert details and update StockLedger per detail line
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    detail.DispatchAdviceHeaderId = entity.Id;
                    await _applicationDbContext.DispatchAdviceDetail.AddAsync(detail);

                    // Update StockLedger: change each PackNo from Packed to Reserved
                    // No BETWEEN — update only records whose StatusId is still Packed (skip already invoiced)
                    for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                    {
                        var stockRecord = await _applicationDbContext.StockLedger
                            .FirstOrDefaultAsync(s => s.UnitId == unitId
                                && s.ItemId == detail.ItemId
                                && s.LotId == detail.LotId
                                && s.PackNo == packNo
                                && s.StatusId == packedStatusId);

                        if (stockRecord != null)
                        {
                            stockRecord.StatusId = reservedStatusId;
                        }
                    }
                }
                await _applicationDbContext.SaveChangesAsync();
            }

            return entity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct)
        {
            var existing = await _applicationDbContext.DispatchAdviceHeader
                .Include(h => h.DispatchAdviceDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            // Get UnitId from the linked SalesOrder
            var unitId = await _applicationDbContext.SalesOrderHeader
                .Where(s => s.Id == existing.SalesOrderId && s.IsDeleted == IsDelete.NotDeleted)
                .Select(s => s.UnitId)
                .FirstOrDefaultAsync(ct);

            // Reverse StockLedger: change each PackNo from Reserved back to Packed
            if (existing.DispatchAdviceDetails != null && existing.DispatchAdviceDetails.Count > 0)
            {
                foreach (var detail in existing.DispatchAdviceDetails)
                {
                    for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                    {
                        var stockRecord = await _applicationDbContext.StockLedger
                            .FirstOrDefaultAsync(s => s.UnitId == unitId
                                && s.ItemId == detail.ItemId
                                && s.LotId == detail.LotId
                                && s.PackNo == packNo
                                && s.StatusId == reservedStatusId, ct);

                        if (stockRecord != null)
                        {
                            stockRecord.StatusId = packedStatusId;
                        }
                    }
                }
            }

            // Soft delete header
            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.DispatchAdviceHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

    }
}
