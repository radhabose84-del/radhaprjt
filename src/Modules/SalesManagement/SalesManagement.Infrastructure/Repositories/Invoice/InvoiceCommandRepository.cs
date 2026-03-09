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

        public async Task<int> UpdateAsync(InvoiceHeader entity)
        {
            var existing = await _dbContext.InvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update updatable fields — preserve immutable: InvoiceNo, DispatchAdviceId, PartyId, UnitId, FinancialYearId
            existing.InvoiceDate             = entity.InvoiceDate;
            existing.InvoiceType             = entity.InvoiceType;
            existing.AgentId                 = entity.AgentId;
            existing.TransportMode           = entity.TransportMode;
            existing.VehicleNumber           = entity.VehicleNumber;
            existing.TransporterName         = entity.TransporterName;
            existing.LRNumber                = entity.LRNumber;
            existing.LRDate                  = entity.LRDate;
            existing.TotalBags               = entity.TotalBags;
            existing.TotalWeight             = entity.TotalWeight;
            existing.TaxableValue            = entity.TaxableValue;
            existing.Discount                = entity.Discount;
            existing.Freight                 = entity.Freight;
            existing.Insurance               = entity.Insurance;
            existing.HandlingCharge          = entity.HandlingCharge;
            existing.OtherCharges            = entity.OtherCharges;
            existing.CGST                    = entity.CGST;
            existing.SGST                    = entity.SGST;
            existing.IGST                    = entity.IGST;
            existing.TaxAmount               = entity.TaxAmount;
            existing.TCSPercentage           = entity.TCSPercentage;
            existing.TCS                     = entity.TCS;
            existing.RoundOff                = entity.RoundOff;
            existing.InvoiceAmountBeforeTCS  = entity.InvoiceAmountBeforeTCS;
            existing.InvoiceAmount           = entity.InvoiceAmount;
            existing.Remarks                 = entity.Remarks;
            existing.IsActive                = entity.IsActive;

            // Replace detail lines: delete existing, insert new
            var existingDetails = _dbContext.InvoiceDetail.Where(d => d.InvoiceHeaderId == existing.Id);
            _dbContext.InvoiceDetail.RemoveRange(existingDetails);

            if (entity.InvoiceDetails != null && entity.InvoiceDetails.Count > 0)
            {
                foreach (var detail in entity.InvoiceDetails)
                {
                    detail.Id = 0;
                    detail.InvoiceHeaderId = existing.Id;
                    await _dbContext.InvoiceDetail.AddAsync(detail);
                }
            }

            _dbContext.InvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

    }
}
