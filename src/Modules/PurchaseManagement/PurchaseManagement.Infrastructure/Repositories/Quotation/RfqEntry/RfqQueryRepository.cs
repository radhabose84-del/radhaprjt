#nullable disable
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry
{
    public class RfqQueryRepository : IRfqQueryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ip;
        public RfqQueryRepository(ApplicationDbContext db, IIPAddressService ip, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _db = db;
            _ip = ip;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public Task<RfqMaster> GetAggregateAsync(int id, CancellationToken ct = default) =>
              _db.Rfqs
              .AsNoTracking()
              .Include(r => r.Items)
              .Include(r => r.Suppliers)
              .Include(r => r.RfqStatus)
              .Include(r => r.InitiationType)
              .FirstOrDefaultAsync(r => r.Id == id, ct);
      
        public async Task<(IReadOnlyList<RfqListItemDto> Items, int Total)> GetAllAsync(int page, int pageSize, int? statusId, string searchTerm, CancellationToken ct)
        {
            var unitId = _ip.GetUnitId() ?? 0;
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var q = _db.Rfqs
                .AsNoTracking()
                .Include(r => r.RfqStatus)
                .Include(r => r.InitiationType)
                .AsQueryable();

            if (statusId.HasValue)
                q = q.Where(r => r.RfqStatusId == statusId.Value);

            q = q.Where(r => r.UnitId == unitId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var trimmed = searchTerm.Trim();
                var s = $"%{trimmed}%";
                var upper = trimmed.ToUpper();

                var isOpenFilter = upper == "OPEN";
                var isClosedFilter = upper == "CLOSED";

                q = q.Where(r =>
                    EF.Functions.Like(r.RfqCode, s) ||
                    r.Suppliers.Any(rs => EF.Functions.Like(rs.Name, s)) ||

                    (isOpenFilter &&
                        !r.QuotationRfq.Any(qh =>
                            qh.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                            qh.IsActive == BaseEntity.Status.Active)) ||

                    (isClosedFilter &&
                        r.QuotationRfq.Any(qh =>
                            qh.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                            qh.IsActive == BaseEntity.Status.Active))
                );
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    Rfq = r,
                    HasActiveQuotation = r.QuotationRfq.Any(qh =>
                        qh.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                        qh.IsActive == BaseEntity.Status.Active)
                })
                .Select(x => new RfqListItemDto(
                    x.Rfq.Id,
                    x.Rfq.UnitId,
                    x.Rfq.RfqCode,
                    x.Rfq.RfqStatusId,
                    x.Rfq.RfqStatus != null ? x.Rfq.RfqStatus.Description : string.Empty,
                    x.Rfq.InitiationTypeId,
                    x.Rfq.InitiationType != null ? x.Rfq.InitiationType.Description : string.Empty,
                    x.Rfq.IndentId,
                    x.Rfq.LastSubmitDate,
                    x.HasActiveQuotation ? 1 : 0,
                    x.HasActiveQuotation ? "Quotation details exist" : null,
                    x.HasActiveQuotation ? "Closed" : "Open"
                ))
                .ToListAsync(ct);

            return (items, total);
        }


        public async Task<List<RfqAutoCompleteDto>> GetRfqAutoCompleteAsync(
        string searchPattern,
        DateOnly? lastSubmitDate,    // optional override
        CancellationToken ct)
        {
            var unitId = _ip.GetUnitId() ?? 0;
            var date = lastSubmitDate ?? DateOnly.FromDateTime(DateTime.Now);
            var q = _db.Set<RfqMaster>()
                .AsNoTracking()
                .Where(r => r.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                            r.IsActive == BaseEntity.Status.Active && r.UnitId == unitId
                            && r.RfqStatus.Code == MiscEnumEntity.Submit.ToString() &&
                            (r.RfqStatus.Code == "Submit") &&
                            r.LastSubmitDate.HasValue &&
                            r.LastSubmitDate.Value >= date);

            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                var term = searchPattern.Trim();
                q = q.Where(r => r.RfqCode.Contains(term));
            }
            return await q
                .OrderByDescending(r => r.Id)
                .Select(r => new RfqAutoCompleteDto
                {
                    Id = r.Id,
                    RfqCode = r.RfqCode,
                    LastSubmitDate = r.LastSubmitDate
                })
                .ToListAsync(ct);
        }
        public async Task<List<RfqAutoCompleteDto>> GetRfqAutoCompleteQuotationAsync(
            string searchPattern,
            DateOnly? lastSubmitDate,
            CancellationToken ct/*, int? supplierId = null*/)
        {
            var unitId = _ip.GetUnitId() ?? 0;
            var date = lastSubmitDate ?? DateOnly.FromDateTime(DateTime.Now);
            var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus,
                MiscEnumEntity.Pending);
       
            var q = _db.Set<RfqMaster>()
                .AsNoTracking()
                .Where(r =>
                    r.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                    r.IsActive  == BaseEntity.Status.Active &&
                    r.UnitId    == unitId &&
                    r.RfqStatus.Code == MiscEnumEntity.Submit.ToString() &&
                    r.LastSubmitDate.HasValue &&
                    r.LastSubmitDate.Value >= date &&                    
                    !_db.Set<QuotationHeader>().Any(qh =>
                        qh.RfqId == r.Id &&
                        qh.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                        qh.IsActive  == BaseEntity.Status.Active)
                    && !_db.Set<QuotationComparisonHeader>().Any(qch =>
                        qch.RfqId == r.Id &&                
                        qch.StatusId== pending.Id));


                    if (!string.IsNullOrWhiteSpace(searchPattern))
                    {
                        var term = searchPattern.Trim();
                        q = q.Where(r => r.RfqCode.Contains(term));
                    }

            return await q
                .OrderByDescending(r => r.Id)
                .Select(r => new RfqAutoCompleteDto
                {
                    Id             = r.Id,
                    RfqCode        = r.RfqCode,
                    LastSubmitDate = r.LastSubmitDate
                })
                .ToListAsync(ct);
        }
        public async Task<List<RfqAutoCompleteDto>> GetRfqAutoCompleteComparisonAsync(
            string searchPattern,
            DateOnly? lastSubmitDate,
            int? statusId,
            CancellationToken ct)
        {
            var unitId = _ip.GetUnitId() ?? 0;

            // Pending status row (from Misc master)
            var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus,
                MiscEnumEntity.Pending);

            var date = lastSubmitDate ?? DateOnly.FromDateTime(DateTime.Now);

            // Base RFQ query – no LastSubmitDate filter here
            var q = _db.Set<RfqMaster>()
                .AsNoTracking()
                .Where(r =>
                    r.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                    r.IsActive  == BaseEntity.Status.Active &&
                    r.UnitId    == unitId &&
                    // must have at least one active quotation
                    _db.Set<QuotationHeader>().Any(qh =>
                        qh.RfqId     == r.Id &&
                        qh.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                        qh.IsActive  == BaseEntity.Status.Active)
                );
            
            var comparisonHeaders = _db.Set<QuotationComparisonHeader>();

            if (statusId.HasValue)
            {
                var sId = statusId.Value;

                if (sId == pending.Id)
                {                  
                    q = q.Where(r =>
                        r.LastSubmitDate.HasValue &&
                        r.LastSubmitDate.Value >= date &&
                        (
                            !comparisonHeaders.Any(ch => ch.RfqId == r.Id) || 
                            comparisonHeaders.Any(ch =>
                                ch.RfqId == r.Id &&
                                ch.StatusId == sId)
                        ));
                }
                else
                {
                    q = q.Where(r =>
                        comparisonHeaders.Any(ch =>
                            ch.RfqId == r.Id &&
                            ch.StatusId == sId));
                }
            }
            else
            {
                var pendingId = pending.Id;

                q = q.Where(r =>
                    r.LastSubmitDate.HasValue &&
                    r.LastSubmitDate.Value >= date &&
                    (
                        !comparisonHeaders.Any(ch => ch.RfqId == r.Id) || // no comparison row
                        comparisonHeaders.Any(ch =>
                            ch.RfqId == r.Id &&
                            ch.StatusId == pendingId)
                    ));
            }

            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                var term = searchPattern.Trim();
                q = q.Where(r => r.RfqCode.Contains(term));
            }

            return await q
                .OrderByDescending(r => r.Id)
                .Select(r => new RfqAutoCompleteDto
                {
                    Id             = r.Id,
                    RfqCode        = r.RfqCode,
                    LastSubmitDate = r.LastSubmitDate
                })
                .ToListAsync(ct);
        }



        public async Task<IReadOnlyList<OpenRfqConflictDto>> FindBlockingSupplierItemPairsAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> supplierIds,
            int unitId,
            DateOnly now,
            int? excludingRfqId = null,
            CancellationToken ct = default)
        {
            var items = itemIds?.Distinct().ToArray() ?? Array.Empty<int>();
            var sups = supplierIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (items.Length == 0 || sups.Length == 0) return Array.Empty<OpenRfqConflictDto>();

            // “Blocking” = LastSubmitDate not null and >= now (i.e., BEFORE last submission date)
            var q =
                from m in _db.Set<RfqMaster>().AsNoTracking()
                #pragma warning disable CS0472
                where m.UnitId == unitId
                #pragma warning restore CS0472
                #pragma warning disable CS0472
                && (m.IsDeleted == null || m.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                #pragma warning restore CS0472
                && m.LastSubmitDate != null
                && m.LastSubmitDate >= now
                && (excludingRfqId == null || m.Id != excludingRfqId.Value)
                from it in m.Items
                from sp in m.Suppliers
                where items.Contains(it.ItemId)
                && sp.SupplierId != null && sups.Contains(sp.SupplierId.Value)
                select new OpenRfqConflictDto
                {
                    RfqId = m.Id,
                    RfqCode = m.RfqCode,
                    SupplierId = sp.SupplierId!.Value,
                    ItemId = it.ItemId,
                    LastSubmitDate = m.LastSubmitDate
                };

            // Distinct by (ItemId, SupplierId, RfqId)
            return await q.Distinct().ToListAsync(ct);
        }

       
    }
}
