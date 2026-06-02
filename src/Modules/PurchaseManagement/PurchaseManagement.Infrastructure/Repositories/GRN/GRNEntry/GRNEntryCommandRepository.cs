#nullable disable
using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.GRN.GRNEntry
{
    public class GRNEntryCommandRepository : IGRNEntryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDbConnection _dbConnection;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public GRNEntryCommandRepository(
            ApplicationDbContext applicationDbContext,
            IIPAddressService ipAddressService,
            IDbConnection dbConnection,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
            _dbConnection = dbConnection;
            _documentSequenceLookup = documentSequenceLookup;
        }


        public async Task<int> CreateAsync(GrnHeader grnHeader, int transactionTypeId, CancellationToken ct = default)
        {
            // Mirror PurchaseReturn: insert + DocNo advance in the same transaction so the GrnNo
            // we just consumed can never be reused. Without the atomic increment, every GRN would
            // be generated with the same DocNo.
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();
            var newId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _applicationDbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    await _applicationDbContext.GrnHeader.AddAsync(grnHeader, ct);
                    await _applicationDbContext.SaveChangesAsync(ct);

                    var dbTx = tx.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbTx.Connection!, dbTx);

                    await tx.CommitAsync(ct);
                    newId = grnHeader.Id;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });

            return newId;
        }

        public async Task<int> CreatePutawayListAsync(List<GrnPutAwayRule> putawayList)
        {
            await _applicationDbContext.GrnPutAwayRule.AddRangeAsync(putawayList);
            return await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<int> CreatePutawayListAsync(List<GrnPutAwayRule> putawayList, IDbTransaction transaction)
        {
            const string sql = @"
                INSERT INTO [Purchase].[GrnPutAwayRule]
                    (GrnId, GrnDetailId, PoId, PoSlNoLocal, ItemId, UnitId,
                     PurchaseUomId, StockUomId, ConversionFactor,
                     QcAcceptedQtyPurchaseUom, QcAcceptedQtyStockUom,
                     WarehouseId, StorageTypeId, TargetId,
                     PriorityId, [Override],
                     PutAwayDate, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@GrnId, @GrnDetailId, @PoId, @PoSlNoLocal, @ItemId, @UnitId,
                     @PurchaseUomId, @StockUomId, @ConversionFactor,
                     @QcAcceptedQtyPurchaseUom, @QcAcceptedQtyStockUom,
                     @WarehouseId, @StorageTypeId, @TargetId,
                     @PriorityId, @Override,
                     @PutAwayDate, @CreatedBy, @CreatedDate, @CreatedByName, @CreatedIP)";

            var conn = transaction.Connection ?? _dbConnection;
            var result = await conn.ExecuteAsync(sql, putawayList, transaction: transaction);
            return result;
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _applicationDbContext.Database.BeginTransactionAsync();
        }
        public async Task<int> CreateStockLedgerEntriesAsync(List<StockLedger> stockLedgerList)
        {
            await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerList);
            return await _applicationDbContext.SaveChangesAsync();
        }

        // GenerateNextCodeAsync removed — GrnNo is now produced by the handler through
        // IDocumentSequenceLookup against Finance.DocumentSequence (per-unit, per-financial-year).


        public async Task<bool> UpdateAsync(int Id, GrnHeader grnHeader)
        {

            var existingHeader = await _applicationDbContext.GrnHeader
            .Include(p => p.GrnDetails)
            .FirstOrDefaultAsync(p => p.Id == Id);


            if (existingHeader == null)
                return false; // Not found

            if (!existingHeader.IsGrnGenerated)
            {
                // Always update header fields
                existingHeader.InvoiceNo = grnHeader.InvoiceNo;
                existingHeader.InvoiceDate = grnHeader.InvoiceDate;
                existingHeader.DcNo = grnHeader.DcNo;
                existingHeader.DcDate = grnHeader.DcDate;
                existingHeader.ReceivingWarehouseId = grnHeader.ReceivingWarehouseId;
                existingHeader.Remarks = grnHeader.Remarks;
                existingHeader.ModifiedBy = _ipAddressService.GetUserId();
                existingHeader.ModifiedDate = DateTimeOffset.Now;
                existingHeader.ModifiedByName = _ipAddressService.GetUserName();
                existingHeader.ModifiedIP = _ipAddressService.GetUserIPAddress();
                existingHeader.CGSTTotal= grnHeader.CGSTTotal;
                existingHeader.SGSTTotal= grnHeader.SGSTTotal;
                existingHeader.IGSTTotal= grnHeader.IGSTTotal;
                existingHeader.DiscountTotal= grnHeader.DiscountTotal;
                existingHeader.ItemsTotal= grnHeader.ItemsTotal;
                existingHeader.TaxableAmount= grnHeader.TaxableAmount;
                existingHeader.MiscCharges= grnHeader.MiscCharges;
                existingHeader.PurchaseValue= grnHeader.PurchaseValue;


                // Always update GRN details
                foreach (var detailDto in grnHeader.GrnDetails)
                {
                    var existingDetail = existingHeader.GrnDetails.FirstOrDefault(d => d.Id == detailDto.Id && d.GrnId == Id && d.PoId == detailDto.PoId && d.PoSlNoLocal == detailDto.PoSlNoLocal);
                    if (existingDetail != null)
                    {
                        existingDetail.DcQuantity = detailDto.DcQuantity;
                        existingDetail.ReceivedQuantity = detailDto.ReceivedQuantity;
                        existingDetail.CGST= detailDto.CGST;
                        existingDetail.SGST= detailDto.SGST;
                        existingDetail.IGST= detailDto.IGST;
                    
                        
                    }
                }

                // Always update IsGrnGenerated
                existingHeader.IsGrnGenerated = grnHeader.IsGrnGenerated;
            }

            // QC Stage: QC sign-off is now per-line (header-level QC columns moved to GrnDetail).
            // Per-line gating "don't re-approve an already-approved line" replaces the prior
            // header-level gate.
            if (existingHeader.IsGrnGenerated)
            {
                existingHeader.QcWarehouseId = grnHeader.QcWarehouseId;
                existingHeader.RejectedImage = grnHeader.RejectedImage;

                var userName = _ipAddressService.GetUserName();
                var userIp = _ipAddressService.GetUserIPAddress();

                foreach (var detailDto in grnHeader.GrnDetails)
                {
                    var existingDetail = existingHeader.GrnDetails.FirstOrDefault(d => d.Id == detailDto.Id && d.GrnId == Id && d.PoId == detailDto.PoId && d.PoSlNoLocal == detailDto.PoSlNoLocal);
                    if (existingDetail == null) continue;

                    // Qty / remarks always updatable while header is in QC stage.
                    existingDetail.QcAcceptedQuantity = detailDto.QcAcceptedQuantity;
                    existingDetail.QcRejectedQuantity = detailDto.QcRejectedQuantity;
                    existingDetail.QcRejectedRemarks = detailDto.QcRejectedRemarks;

                    // Per-line QC sign-off only updates on lines not already approved.
                    if (!existingDetail.IsQcApproved)
                    {
                        existingDetail.QcRemarks    = detailDto.QcRemarks;
                        existingDetail.QcPersonName = userName;
                        existingDetail.QcStatusId   = detailDto.QcStatusId;
                        existingDetail.QcDate       = DateTimeOffset.Now;
                        existingDetail.QcApprovedIp = userIp;
                        existingDetail.IsQcApproved = detailDto.IsQcApproved;
                    }
                }
            }

            _applicationDbContext.GrnHeader.Update(existingHeader);
            var updatedRows = await _applicationDbContext.SaveChangesAsync();

            return updatedRows > 0; // true if any row was updated
        }

        public async Task<int> CreatePutawayWithStockLedgerAsync(List<GrnPutAwayRule> putawayList, List<StockLedger> stockLedgerList, Func<Task> publishEvents)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                await _applicationDbContext.GrnPutAwayRule.AddRangeAsync(putawayList);
                await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerList);
                await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                if (publishEvents != null)
                    await publishEvents();

                return putawayList.Count;
            });
        }
        public async Task<GrnHeader> GetGrnHeaderAsync(int grnId, CancellationToken ct = default)
        {
            return await _applicationDbContext.Set<GrnHeader>()
                .FirstOrDefaultAsync(x => x.Id == grnId, ct);
        }

        // 🔹 NEW: Get GRN detail list by GrnId
        public async Task<List<GrnDetail>> GetGrnDetailsByGrnIdAsync(int grnId, CancellationToken ct = default)
        {
            return await _applicationDbContext.Set<GrnDetail>()
                .Where(x => x.GrnId == grnId)
                .ToListAsync(ct);
        }
          public async Task<GrnHeader> GetGrnWithDetailsAsync(int grnId, CancellationToken ct = default)
        {
            return await _applicationDbContext.Set<GrnHeader>()
                .Include(x => x.GrnDetails)   
                .FirstOrDefaultAsync(x => x.Id == grnId, ct);
        }

        public async Task<bool> UpdateAsync(int id, GrnHeader grnHeader,List<CalculatedDetail> calculatedDetails,List<UpdateGRNEntryDto.UpdateGRNDetailsDto> detailDtos)
        {
             var existingHeader = await _applicationDbContext.GrnHeader
                .Include(p => p.GrnDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingHeader == null)
                return false;

            // --------------------------
            // 1) HEADER UPDATE (Before GRN generation)
            // --------------------------
            if (!existingHeader.IsGrnGenerated)
            {
                existingHeader.InvoiceNo = grnHeader.InvoiceNo;
                existingHeader.InvoiceDate = grnHeader.InvoiceDate;
                existingHeader.DcNo = grnHeader.DcNo;
                existingHeader.DcDate = grnHeader.DcDate;
                existingHeader.ReceivingWarehouseId = grnHeader.ReceivingWarehouseId;
                existingHeader.Remarks = grnHeader.Remarks;

                existingHeader.CGSTTotal = grnHeader.CGSTTotal;
                existingHeader.SGSTTotal = grnHeader.SGSTTotal;
                existingHeader.IGSTTotal = grnHeader.IGSTTotal;
                existingHeader.DiscountTotal = grnHeader.DiscountTotal;
                existingHeader.ItemsTotal = grnHeader.ItemsTotal;
                existingHeader.TaxableAmount = grnHeader.TaxableAmount;
                existingHeader.MiscCharges = grnHeader.MiscCharges;
                existingHeader.PurchaseValue = grnHeader.PurchaseValue;

                existingHeader.ModifiedBy = _ipAddressService.GetUserId();
                existingHeader.ModifiedDate = DateTimeOffset.Now;
                existingHeader.ModifiedByName = _ipAddressService.GetUserName();
                existingHeader.ModifiedIP = _ipAddressService.GetUserIPAddress();

                // Update only Qty fields in detail
                foreach (var dto in detailDtos)
                {
                    var detail = existingHeader.GrnDetails.FirstOrDefault(d => d.Id == dto.Id);
                    if (detail != null)
                    {
                        detail.DcQuantity = dto.DcQuantity;
                        detail.ReceivedQuantity = dto.ReceivedQuantity;
                        detail.ExpiryDate=dto.ExpiryDate;
                        // Apply calculated values
                        var calc = calculatedDetails.FirstOrDefault(c => c.Id == dto.Id);
                        if (calc != null)
                        {
                            detail.CGST = calc.CGST;
                            detail.SGST = calc.SGST;
                            detail.IGST = calc.IGST;
                            detail.ItemValue = calc.ItemValue;
                            detail.TaxableAmount = calc.TaxableAmount;
                            detail.DiscountValue = calc.DiscountValue;

                            // Only set when a new/finalized image is supplied — never clear an existing one
                            if (!string.IsNullOrWhiteSpace(calc.GrnDetailImage))
                                detail.GrnDetailImage = calc.GrnDetailImage;
                        }
                        
                    }
                }

                existingHeader.IsGrnGenerated = grnHeader.IsGrnGenerated;
            }

            // --------------------------
            // 2) QC UPDATE — per-line (sign-off fields moved from header to GrnDetail)
            // --------------------------
            if (existingHeader.IsGrnGenerated)
            {
                existingHeader.QcWarehouseId = grnHeader.QcWarehouseId;
                existingHeader.RejectedImage = grnHeader.RejectedImage;

                var userName = _ipAddressService.GetUserName();
                var userIp = _ipAddressService.GetUserIPAddress();

                foreach (var dto in detailDtos)
                {
                    var detail = existingHeader.GrnDetails.FirstOrDefault(d => d.Id == dto.Id);
                    if (detail == null) continue;

                    // Qty / remarks / expiry always updatable while header is in QC stage.
                    detail.QcAcceptedQuantity = dto.QcAcceptedQuantity ?? 0;
                    detail.QcRejectedQuantity = dto.QcRejectedQuantity ?? 0;
                    detail.QcRejectedRemarks  = dto.QcRejectedRemarks;
                    detail.ExpiryDate         = dto.ExpiryDate;

                    // Per-line QC sign-off only updates on lines not already approved.
                    if (!detail.IsQcApproved)
                    {
                        detail.QcRemarks    = dto.QcRemarks;
                        detail.QcPersonName = userName;
                        detail.QcStatusId   = dto.QcStatusId;
                        detail.QcDate       = DateTimeOffset.Now;
                        detail.QcApprovedIp = userIp;
                        detail.IsQcApproved = dto.IsQcApproved == 1;
                    }
                }
            }

            _applicationDbContext.GrnHeader.Update(existingHeader);
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }
    }
}