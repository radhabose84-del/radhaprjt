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
       
        public async Task<int> CreateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId, int typeId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var details = entity.InvoiceDetails?.ToList();
                    entity.InvoiceDetails = null;

                    await _dbContext.InvoiceHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.InvoiceHeaderId = entity.Id;
                            await _dbContext.InvoiceDetail.AddAsync(detail);
                        }
                        await _dbContext.SaveChangesAsync();
                    }

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

                    // Increment DocNo in Finance.DocumentSequence
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [Finance].[DocumentSequence] SET DocNo = DocNo + 1 WHERE TypeId = {0} AND IsDeleted = 0",
                        typeId);

                    await _dbContext.SaveChangesAsync();
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

        public async Task<int> UpdateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId)
        {
            var existing = await _dbContext.InvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Step 1: Revert old StockLedger entries (Invoiced → Dispatched)
                    var daDetails = await _dbContext.DispatchAdviceDetail
                        .Where(d => d.DispatchAdviceHeaderId == existing.DispatchAdviceId)
                        .ToListAsync();

                    foreach (var daDetail in daDetails)
                    {
                        var stockRecords = await _dbContext.StockLedger
                            .Where(s => s.UnitId == unitId
                                && s.ItemId == daDetail.ItemId
                                && s.LotId == daDetail.LotId
                                && s.PackNo >= daDetail.StartPackNo
                                && s.PackNo <= daDetail.EndPackNo
                                && s.StatusId == invoicedStatusId)
                            .ToListAsync();

                        foreach (var stock in stockRecords)
                            stock.StatusId = dispatchedStatusId;
                    }

                    await _dbContext.SaveChangesAsync();

                    // Step 2: Update updatable fields — preserve immutable: InvoiceNo, DispatchAdviceId, PartyId, UnitId, FinancialYearId
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

                    // Step 3: Replace detail lines: delete existing, insert new
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

                    // Step 4: Re-apply StockLedger entries (Dispatched → Invoiced)
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

                    _dbContext.InvoiceHeader.Update(existing);
                    await _dbContext.SaveChangesAsync();
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

    }
}
