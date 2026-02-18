using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Services;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationCompare
{
    public class QuotationCompareCommandRepository : IQuotationCompareCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public QuotationCompareCommandRepository(ApplicationDbContext dbContext, IIPAddressService ipAddressService, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _ipAddressService = ipAddressService;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }
        public async Task<int> AddAsync(QuotationComparisonHeader entity)
        {
            // Fetch Pending StatusId
            var pendingStatusId = await (
                        from m in _dbContext.MiscMaster
                        join mt in _dbContext.MiscTypeMaster
                            on m.MiscTypeId equals mt.Id
                        where mt.MiscTypeCode == "ApprovalStatus" && m.Code == "Pending"
                        select m.Id
                    ).FirstOrDefaultAsync();

            entity.StatusId = pendingStatusId;
            entity.CreatedBy = _ipAddressService.GetUserId();
            entity.CreatedByName = _ipAddressService.GetUserName();
            entity.CreatedDate = DateTime.Now;
            entity.CreatedIP = _ipAddressService.GetSystemIPAddress();
            entity.ConfirmedDate = DateTime.Now;

            // Add main PartyMaster
            await _dbContext.QuotationComparisonHeader.AddAsync(entity);

            // EF will automatically save non-null child collections
            await _dbContext.SaveChangesAsync();

            return entity.Id; ;
        }

        public async Task<bool> ExistsAsync(int rfqId, string rfqCode)
        {
            return await _dbContext.QuotationComparisonHeader
                .AnyAsync(q => q.RfqId == rfqId && q.RfqCode == rfqCode);
        }
        public async Task<QuoteComparisonWorkFlowDto> GetByIdQuoteComparisonWorkFlowAsync(int id)
        {
            var entity = await _dbContext.QuotationComparisonHeader
            .Where(x => x.Id == id)
            .Select(x => new QuoteComparisonWorkFlowDto
            {
                Id = x.Id,
                RfqCode = x.RfqCode,
                RfqId = x.RfqId,
                StatusId = x.StatusId,
                UnitId = _ipAddressService.GetUnitId()
                //RfqHeaderId = x.RfqHeaderId,
                //OverrideStatus = x.OverrideStatus                           
            })
            .FirstOrDefaultAsync();

            return entity!;
        }
       /*  public async Task<bool> UpdateQuoteApproveAsync(int id, int statusId, CancellationToken ct = default)
        {
           var existingParty = await _dbContext.QuotationComparisonHeader
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingParty != null)
            {                
                
                existingParty.StatusId = statusId;

                // Save changes
                return await _dbContext.SaveChangesAsync() > 0;
            }

            return false;
            
        } */
        public async Task<bool> UpdateQuoteApproveAsync(int comparisonHeaderId, int statusId, CancellationToken ct = default)
        {
            // 1) Lookup Approved/Rejected ids from Misc
            var approved = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
            var rejected = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

            // 2) Load comparison header
            var qch = await _dbContext.QuotationComparisonHeader
                .FirstOrDefaultAsync(h => h.Id == comparisonHeaderId, ct);
            if (qch is null) return false;

            // 3) Update status on header
            qch.StatusId = statusId;

            // 4) If approved → build price masters
            if (statusId == approved.Id)
            {
                await CreatePriceMastersFromComparisonAsync(comparisonHeaderId, ct);
            }

            return await _dbContext.SaveChangesAsync(ct) > 0;
        }

        private async Task CreatePriceMastersFromComparisonAsync(int comparisonHeaderId, CancellationToken ct)
        {
            //var unitId = _ipAddressService.GetUnitId();
            var nowDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // Resolve SourceFromId for "Quotation"
            var srcFrom = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.SourceFrom, MiscEnumEntity.Quotation);
            var sourceFromId = srcFrom.Id;

            // Selected rows from comparison detail (must have QuotationDetailId)
            var rows = await _dbContext.QuotationComparisonDetail
                .Where(d => d.QuotationComparisonHeaderId == comparisonHeaderId &&
                            #pragma warning disable CS0472
                            d.QuotationDetailId != null)
                            #pragma warning restore CS0472
                .Select(d => new { d.QuotationDetailId, d.QuotationHeaderId })
                .ToListAsync(ct);

            if (rows.Count == 0) return;

            var qdIds = rows.Select(r => r.QuotationDetailId).Distinct().ToList();

            // Avoid duplicates: skip any QD already used as SourceDetailId
            var alreadyUsed = await _dbContext.Set<PriceMasterHeader>()
                .Where(p => qdIds.Contains(p.SourceDetailId ?? 0))
                .Select(p => p.SourceDetailId!.Value)
                .Distinct()
                .ToListAsync(ct);

            var qdIdsToCreate = qdIds.Except(alreadyUsed).ToList();
            if (qdIdsToCreate.Count == 0) return;

            // Fetch QuotationDetail + linked header
            var qDetails = await _dbContext.Set<QuotationDetail>()
                .Where(qd => qdIdsToCreate.Contains(qd.Id))
                .Select(qd => new
                {
                    qd.Id,                
                    qd.ItemId,
                    qd.UomId,
                    qd.CurrencyId,
                    qd.Rate,
                    qd.QuotationHeaderId
                })
                .ToListAsync(ct);

            var headerIds = qDetails.Select(x => x.QuotationHeaderId).Distinct().ToList();

            var qHeaders = await _dbContext.Set<QuotationHeader>()
                .Where(h => headerIds.Contains(h.Id))
                .Select(h => new
                {
                    h.Id,
                    SupplierId = h.SupplierId,
                    ValidTill = h.ValidTill   ,h.UnitId                 
                })
                .ToDictionaryAsync(x => x.Id, x => x, ct);
            
            var approvedStatusId = await (
                        from m in _dbContext.MiscMaster
                        join mt in _dbContext.MiscTypeMaster
                            on m.MiscTypeId equals mt.Id
                        where mt.MiscTypeCode == MiscEnumEntity.ApprovalStatus && m.Code == MiscEnumEntity.Approved
                        select m.Id
                    ).FirstOrDefaultAsync();

            // Build headers & details
            foreach (var qd in qDetails)
            {
                if (!qHeaders.TryGetValue(qd.QuotationHeaderId, out var qh)) continue;

                var header = new PriceMasterHeader
                {
                    UnitId = qh.UnitId,
                    ItemId = qd.ItemId,
                    VendorId = qh.SupplierId,
                    UomId = qd.UomId,
                    ValidFrom = nowDate,
                    ValidTo = qh.ValidTill,
                    StatusId = approvedStatusId,
                    SourceFromId = sourceFromId,
                    SourceDetailId = qd.Id,
                    IsActive = BaseEntity.Status.Active                    
                };

                header.Details.Add(new PriceMasterDetail
                {
                    ScaleQtyFrom = 1m,
                    ScaleQtyTo = null,
                    UnitPrice = qd.Rate,
                    CurrencyId = qd.CurrencyId,
                    IsActive = BaseEntity.Status.Active,
                    CreatedBy = _ipAddressService.GetUserId(),
                    CreatedByName=_ipAddressService.GetUserName()
                });                

          
                await _dbContext.Set<PriceMasterHeader>().AddAsync(header, ct);
            }
        }
    }
}