using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.Invoice
{
    public class InvoiceCommandRepository : IInvoiceCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public InvoiceCommandRepository(
            ApplicationDbContext dbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _dbContext = dbContext;
            _documentSequenceLookup = documentSequenceLookup;
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

                    // Update DispatchAdviceHeader.InvFlg = true
                    var dispatchAdvice = await _dbContext.DispatchAdviceHeader
                        .FirstOrDefaultAsync(d => d.Id == entity.DispatchAdviceId && d.IsDeleted == IsDelete.NotDeleted);
                    if (dispatchAdvice != null)
                    {
                        dispatchAdvice.InvFlg = true;
                        _dbContext.DispatchAdviceHeader.Update(dispatchAdvice);
                    }

                    var dbConnection = _dbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(typeId, dbConnection, dbTransaction);

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
                    existing.AgentId                 = entity.AgentId;
                    existing.InvoiceTypeId           = entity.InvoiceTypeId;
                    existing.TransportMode           = entity.TransportMode;
                    existing.VehicleNumber           = entity.VehicleNumber;
                    existing.TransporterName         = entity.TransporterName;
                    existing.LRNumber                = entity.LRNumber;
                    existing.LRDate                  = entity.LRDate;
                    existing.TotalBags               = entity.TotalBags;
                    existing.TotalWeight             = entity.TotalWeight;
                    existing.TaxableValue            = entity.TaxableValue;
                    existing.TotalDiscount            = entity.TotalDiscount;
                    existing.TotalFreight             = entity.TotalFreight;
                    existing.TotalCommission           = entity.TotalCommission;
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
                    existing.GEFlag                  = entity.GEFlag;
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

        public async Task<string?> UpdateApprovalStatusAsync(int id, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct)
        {
            // Resolve the MiscMaster Id scoped to ApprovalStatus type
            var statusMisc = await _dbContext.MiscMaster
                .Include(m => m.MiscTypeMaster)
                .FirstOrDefaultAsync(m => m.Code == status
                    && m.MiscTypeMaster!.MiscTypeCode == "ApprovalStatus"
                    && m.IsDeleted == IsDelete.NotDeleted, ct);

            if (statusMisc == null) return null;

            // On Approval: generate final invoice number + update status in one transaction
            if (status == SalesManagement.Domain.Common.MiscEnumEntity.InvoiceStatusApproved)
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                    try
                    {
                        // Get UnitId from the invoice record
                        var unitId = await _dbContext.InvoiceHeader
                            .Where(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted)
                            .Select(x => x.UnitId)
                            .FirstOrDefaultAsync(ct);

                        // Resolve "Invoice" transaction type for the unit
                        var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                            SalesManagement.Domain.Common.MiscEnumEntity.TransactionTypeInvoice,
                            SalesManagement.Domain.Common.MiscEnumEntity.ModuleSales, unitId);

                        if (!typeId.HasValue)
                            throw new ExceptionRules("Transaction Type 'Invoice' not found for Sales module.");

                        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
                        var invoiceNo = sequences.Count > 0 ? sequences[^1] : null;
                        if (invoiceNo == null)
                            throw new ExceptionRules("No document sequence configured for Invoice.");

                        // Update StatusId + InvoiceNo in one SQL statement
                        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                            UPDATE Sales.InvoiceHeader
                            SET StatusId       = {statusMisc.Id},
                                InvoiceNo      = {invoiceNo},
                                ModifiedBy     = {modifiedBy},
                                ModifiedByName = {modifiedByName},
                                ModifiedIP     = {modifiedIP},
                                ModifiedDate   = SYSDATETIMEOFFSET()
                            WHERE Id = {id} AND IsDeleted = 0", ct);

                        // Increment the Invoice document sequence
                        var dbConnection = _dbContext.Database.GetDbConnection();
                        var dbTransaction = transaction.GetDbTransaction();
                        await _documentSequenceLookup.IncrementDocNoAsync(typeId.Value, dbConnection, dbTransaction);

                        await transaction.CommitAsync(ct);
                        return invoiceNo;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(ct);
                        throw;
                    }
                });
            }

            // Non-approval (Pending/Rejected): just update StatusId
            // Raw SQL bypasses ApplicationDbContext.UpdateIpFields() which would otherwise
            // overwrite ModifiedBy/Name/IP with consumer-context defaults (0/Anonymous).
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Sales.InvoiceHeader
                SET StatusId       = {statusMisc.Id},
                    ModifiedBy     = {modifiedBy},
                    ModifiedByName = {modifiedByName},
                    ModifiedIP     = {modifiedIP},
                    ModifiedDate   = SYSDATETIMEOFFSET()
                WHERE Id = {id} AND IsDeleted = 0", ct);

            return null;
        }

        public async Task UpdateInvoiceStatusIdAsync(int id, int statusId, CancellationToken ct)
        {
            var entity = await _dbContext.InvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (entity == null) return;

            entity.StatusId = statusId;
            _dbContext.InvoiceHeader.Update(entity);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<InvoiceWorkFlowDto> GetByIdInvoiceWorkFlowAsync(int id)
        {
            var entity = await _dbContext.InvoiceHeader
                .Where(x => x.Id == id)
                .Select(x => new InvoiceWorkFlowDto
                {
                    Id = x.Id,
                    InvoiceNo = x.InvoiceNo,
                    StatusId = x.StatusId,
                    StatusName = x.StatusMisc != null ? x.StatusMisc.Description : null,
                    UnitId = x.UnitId
                })
                .FirstOrDefaultAsync();

            return entity!;
        }

    }
}
