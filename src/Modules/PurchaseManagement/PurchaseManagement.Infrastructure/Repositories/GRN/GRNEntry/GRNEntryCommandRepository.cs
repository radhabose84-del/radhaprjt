#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public GRNEntryCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }


        public async Task<int> CreateAsync(GrnHeader grnHeader)
        {
            // First save the header so GrnId is generated
            await _applicationDbContext.GrnHeader.AddAsync(grnHeader);
            await _applicationDbContext.SaveChangesAsync();
            return grnHeader.Id;
        }

        public async Task<int> CreatePutawayListAsync(List<GrnPutAwayRule> putawayList)
        {
            await _applicationDbContext.GrnPutAwayRule.AddRangeAsync(putawayList);
            return await _applicationDbContext.SaveChangesAsync();
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

        public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitCode = unitId > 0 ? unitId.ToString() : "NA";
            var prefix = $"GRN-{unitCode}-";

            var recent = await _applicationDbContext.GrnHeader.AsNoTracking()
                .Where(r => r.GrnNo.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.GrnNo)
                .Take(100)
                .ToListAsync(ct);

            var max = 0;
            foreach (var code in recent)
            {
                var suffix = code.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n) && n > max) max = n;
            }

            return $"{prefix}{(max + 1):D2}";
        }


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

            // QC Stage: update only if QC not approved yet
            if (!existingHeader.IsQcApproved && existingHeader.IsGrnGenerated)
            {
                existingHeader.QcRemarks = grnHeader.QcRemarks;
                existingHeader.QcPersonName = _ipAddressService.GetUserName();
                existingHeader.QcStatusId = grnHeader.QcStatusId;
                existingHeader.QcDate = DateTimeOffset.Now;
                existingHeader.QcApprovedIp = _ipAddressService.GetUserIPAddress();
                existingHeader.QcWarehouseId = grnHeader.QcWarehouseId;
                existingHeader.RejectedImage = grnHeader.RejectedImage;


                // Always update GRN details
                foreach (var detailDto in grnHeader.GrnDetails)
                {
                    var existingDetail = existingHeader.GrnDetails.FirstOrDefault(d => d.Id == detailDto.Id && d.GrnId == Id && d.PoId == detailDto.PoId && d.PoSlNoLocal == detailDto.PoSlNoLocal);
                    if (existingDetail != null)
                    {
                        existingDetail.QcAcceptedQuantity = detailDto.QcAcceptedQuantity;
                        existingDetail.QcRejectedQuantity = detailDto.QcRejectedQuantity;
                        existingDetail.QcRejectedRemarks = detailDto.QcRejectedRemarks;
                    }
                }

                existingHeader.IsQcApproved = grnHeader.IsQcApproved; // always update flag
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
                        }
                        
                    }
                }

                existingHeader.IsGrnGenerated = grnHeader.IsGrnGenerated;
            }

            // --------------------------
            // 2) QC UPDATE (Only QC values)
            // --------------------------
            if (!existingHeader.IsQcApproved && existingHeader.IsGrnGenerated)
            {
                existingHeader.QcRemarks = grnHeader.QcRemarks;
                existingHeader.QcPersonName = _ipAddressService.GetUserName();
                existingHeader.QcStatusId = grnHeader.QcStatusId;
                existingHeader.QcDate = DateTimeOffset.Now;
                existingHeader.QcApprovedIp = _ipAddressService.GetUserIPAddress();
                existingHeader.QcWarehouseId = grnHeader.QcWarehouseId;
                existingHeader.RejectedImage = grnHeader.RejectedImage;

                // Update QC values
                foreach (var dto in detailDtos)
                {
                    var detail = existingHeader.GrnDetails.FirstOrDefault(d => d.Id == dto.Id);
                    if (detail != null)
                    {
                        detail.QcAcceptedQuantity = dto.QcAcceptedQuantity ?? 0;
                        detail.QcRejectedQuantity = dto.QcRejectedQuantity ?? 0;
                        detail.QcRejectedRemarks = dto.QcRejectedRemarks;
                        detail.ExpiryDate = dto.ExpiryDate;
                    }
                }

                existingHeader.IsQcApproved = grnHeader.IsQcApproved;
            }

            _applicationDbContext.GrnHeader.Update(existingHeader);
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }
    }
}