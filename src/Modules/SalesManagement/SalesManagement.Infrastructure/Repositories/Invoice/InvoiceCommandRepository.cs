using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.Invoice
{
    public class InvoiceCommandRepository : IInvoiceCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public InvoiceCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNextInvoiceNoAsync(int unitId, CancellationToken ct = default)
        {
            var prefix = $"INV-{unitId}-";

            var lastInvoice = await _dbContext.InvoiceHeader
                .Where(x => x.InvoiceNo != null && x.InvoiceNo.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.InvoiceNo)
                .FirstOrDefaultAsync(ct);

            int nextSeq = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                    nextSeq = lastSeq + 1;
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId)
        {
            // Separate details from header before insert
            var details = entity.InvoiceDetails?.ToList();
            entity.InvoiceDetails = null;

            // Insert header
            await _dbContext.InvoiceHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            // Insert detail lines
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    detail.InvoiceHeaderId = entity.Id;
                    await _dbContext.InvoiceDetail.AddAsync(detail);
                }
                await _dbContext.SaveChangesAsync();
            }

            // Update StockLedger: Dispatched → Invoiced for all pack records of this DA
            var daDetails = await _dbContext.DispatchAdviceDetail
                .Where(d => d.DispatchAdviceHeaderId == entity.DispatchAdviceId)
                .ToListAsync();

            foreach (var daDetail in daDetails)
            {
                var stockRecords = await _dbContext.StockLedger
                    .Where(s => s.UnitId == unitId
                        && s.ItemId == daDetail.ItemId
                        && s.LotId == daDetail.LotId
                        && s.PackNo >= daDetail.StartPackNo
                        && s.PackNo <= daDetail.EndPackNo
                        && s.StatusId == dispatchedStatusId)
                    .ToListAsync();

                foreach (var stock in stockRecords)
                    stock.StatusId = invoicedStatusId;
            }

            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.InvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.InvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
