using System.Data;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using PurchaseManagement.Domain.PurchaseOrder;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Infrastructure.PurchaseOrder.Local;

public class PurchaseOrderCommandRepository : IPurchaseOrderCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IIPAddressService _ip;
    private readonly IDbConnection _dbConnection;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
    private readonly ILogger<PurchaseOrderCommandRepository> _logger;

    public PurchaseOrderCommandRepository(
        ApplicationDbContext db,
        IIPAddressService ip,
        IDbConnection dbConnection,
        IMiscMasterQueryRepository miscMasterQueryRepository,
        ILogger<PurchaseOrderCommandRepository> logger)
    {
        _db = db;
        _ip = ip;
        _dbConnection = dbConnection;
        _miscMasterQueryRepository = miscMasterQueryRepository;
        _logger = logger;
    }
    public async Task<int> CreateAsync(PurchaseOrderHeader aggregate, CancellationToken ct)
    {
        // Ensure children are new
        foreach (var h in aggregate.Headers ?? Enumerable.Empty<PurchaseLocalHeader>())
        {
            h.Id = 0;
            foreach (var d in h.Details ?? Enumerable.Empty<PurchaseLocalDetail>())
                d.Id = 0;
        }
        foreach (var t in aggregate.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTerm>())
            t.Id = 0;

        // ✅ Ensure document rows are new (if handler attached them)
        foreach (var doc in aggregate.PurchaseDocumentTypes ?? Enumerable.Empty<PurchaseDocument>())
            doc.Id = 0;

        // Status = Pending
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        aggregate.StatusId = pending.Id;

        // Compute impacted indents from the incoming graph
        var impacted = GetImpactedIndentIdsFromPo(aggregate);

        var strategy = _db.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.PurchaseOrderHeaders.Add(aggregate);
            await _db.SaveChangesAsync(ct); // PK available

            // ✅ If documents were attached on the aggregate, ensure FK and save
            if (aggregate.PurchaseDocumentTypes != null && aggregate.PurchaseDocumentTypes.Count > 0)
            {
                foreach (var doc in aggregate.PurchaseDocumentTypes)
                {
                    doc.PoId = aggregate.Id;
                    if (doc.UploadedDate == default) doc.UploadedDate = DateTimeOffset.UtcNow;
                }
                await _db.SaveChangesAsync(ct);
            }

            // Recompute POQty for impacted indents
            if (impacted.Count > 0)
                await RecomputeIndentPoQtyAsync(impacted, ct);

            await tx.CommitAsync(ct);
            return aggregate.Id;
        });
    }

    /// <summary>
    /// Creates a purchase order WITHOUT managing transaction internally.
    /// Caller is responsible for Begin/Commit/Rollback.
    /// </summary>
    public async Task<int> CreateWithoutTransactionAsync(PurchaseOrderHeader aggregate, CancellationToken ct)
    {
        // Ensure children are new
        foreach (var h in aggregate.Headers ?? Enumerable.Empty<PurchaseLocalHeader>())
        {
            h.Id = 0;
            foreach (var d in h.Details ?? Enumerable.Empty<PurchaseLocalDetail>())
                d.Id = 0;
        }
        foreach (var t in aggregate.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTerm>())
            t.Id = 0;

        foreach (var doc in aggregate.PurchaseDocumentTypes ?? Enumerable.Empty<PurchaseDocument>())
            doc.Id = 0;

        // Status = Pending
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        aggregate.StatusId = pending.Id;

        // Add to context (caller will call SaveChangesAsync)
        _db.PurchaseOrderHeaders.Add(aggregate);
        await _db.SaveChangesAsync(ct); // PK available

        // If documents were attached, ensure FK
        if (aggregate.PurchaseDocumentTypes != null && aggregate.PurchaseDocumentTypes.Count > 0)
        {
            foreach (var doc in aggregate.PurchaseDocumentTypes)
            {
                doc.PoId = aggregate.Id;
                if (doc.UploadedDate == default) doc.UploadedDate = DateTimeOffset.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
        }

        // NOTE: Caller should call RecomputeIndentPoQtyAsync after all operations
        return aggregate.Id;
    }

    /// <summary>
    /// Begins a database transaction. Caller must Commit or Rollback.
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        return await _db.Database.BeginTransactionAsync(ct);
    }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return _db.Database.CreateExecutionStrategy();
    }

   public async Task<int> UpdateAsync(PurchaseOrderHeader incoming, PurchaseOrderUpdateDto dto, CancellationToken ct)
    {
        var existing = await _db.PurchaseOrderHeaders
            .Include(x => x.Headers).ThenInclude(h => h.Details)
            .Include(x => x.PaymentTerms)
            .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

        if (existing is null) return 0;

        incoming.StatusId = existing.StatusId;

        // impacted (old + new)
        var oldIndents = existing.Headers.SelectMany(h => h.Details)
            .Select(d => d.IndentId ?? 0).Where(i => i > 0).Distinct().ToHashSet();

        var newIndents = (dto.Headers ?? new List<PurchaseLocalHeaderDto>())
            .SelectMany(h => h.Details ?? new List<PurchaseLocalDetailDto>())
            .Select(d => d.IndentId ?? 0).Where(i => i > 0).Distinct().ToHashSet();

        var impacted = new HashSet<int>(oldIndents);
        impacted.UnionWith(newIndents);

        // Build incoming docs ONCE (do not set Id)
        var incomingDocs = (dto.Documents ?? new List<PurchaseDocumentDto>())
            .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
            .GroupBy(d => d.DocumentId).Select(g => g.First())
            .Select(d => new PurchaseDocument
            {
                PoId        = existing.Id,
                DocumentId  = d.DocumentId,
                FileName    = d.FileName!,
                UploadedDate= d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
            })
            .ToList();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // 1) Update scalars
            _db.Entry(existing).CurrentValues.SetValues(incoming);
            existing.ModifiedDate = DateTimeOffset.UtcNow;

            // 2) Replace local children (details -> headers)
            if (existing.Headers?.Count > 0)
            {
                var oldDetails = existing.Headers.SelectMany(h => h.Details).ToList();
                if (oldDetails.Count > 0) _db.PurchaseLocalDetails.RemoveRange(oldDetails);
                _db.PurchaseLocalHeaders.RemoveRange(existing.Headers);
            }
            if (existing.PaymentTerms?.Count > 0)
                _db.PurchasePaymentTerms.RemoveRange(existing.PaymentTerms);

            // 3) Replace documents (delete then insert ONCE)
            var oldDocs = await _db.PurchaseDocuments.Where(x => x.PoId == existing.Id).ToListAsync(ct);
            if (oldDocs.Count > 0) _db.PurchaseDocuments.RemoveRange(oldDocs);

            // 4) Rebuild from DTO
            foreach (var h in dto.Headers ?? Enumerable.Empty<PurchaseLocalHeaderDto>())
            {
                var hEntity = new PurchaseLocalHeader
                {
                    PurchaseOrderId         = existing.Id,
                    IsPartialReceiptAllowed = h.IsPartialReceiptAllowed,
                    IncotermsId             = h.IncotermsId,
                    ModeOfDispatchId        = h.ModeOfDispatchId,
                    FreightCharges          = h.FreightCharges,
                    TermsId                 = h.TermsId,
                    TermDescription         = h.TermDescription,
                    DeliveryAddress         = h.DeliveryAddress,
                    BillingAddress          = h.BillingAddress,
                    Details = (h.Details ?? new List<PurchaseLocalDetailDto>()).Select(d => new PurchaseLocalDetail
                    {
                        IndentId        = d.IndentId,
                        ItemId          = d.ItemId,
                        ItemSno         = d.ItemSno,
                        UOMId           = d.UOMId,
                        Quantity        = d.Quantity,
                        UnitPrice       = d.UnitPrice,
                        LastPOPrice     = d.LastPOPrice,
                        DiscountTypeId  = d.DiscountTypeId,
                        DiscountValue   = d.DiscountValue,
                        PandFType       = d.PandFType,
                        PandFCharge     = d.PandFCharge,
                        OtherCharge     = d.OtherCharge,
                        GSTPercentage   = d.GSTPercentage,
                        CGST            = d.CGST,
                        SGST            = d.SGST,
                        IGST            = d.IGST,                        
                        CGSTPercentage  = (d.IGST > 0) ? d.GSTPercentage / 2 : 0,
                        SGSTPercentage  = (d.IGST > 0) ? d.GSTPercentage / 2 : 0,
                        IGSTPercentage  = (d.IGST > 0) ? 0 : d.GSTPercentage,
                        ScheduleDate    = d.ScheduleDate,
                        DepartmentId    = d.DepartmentId,
                        ItemValue       = d.ItemValue
                    }).ToList()
                };
                _db.PurchaseLocalHeaders.Add(hEntity);
            }


            foreach (var t in dto.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTermDto>())
            {
                _db.PurchasePaymentTerms.Add(new PurchasePaymentTerm
                {
                    PurchaseOrderId = existing.Id,
                    PaymentTermId   = t.PaymentTermId,
                    AdvancePercent  = t.AdvancePercent,
                    CreditDays      = t.CreditDays,
                    PaymentModelId  = t.PaymentModelId,
                    InsuranceId     = t.InsuranceId,
                    InsurancePercent= t.InsurancePercent,
                    InsuranceAmount = t.InsuranceAmount,
                    AdvanceAmount   = t.AdvanceAmount,
                    BalancePercent  = t.BalancePercent,
                    BalanceAmount   = t.BalanceAmount
                });
            }

            
                _db.PurchaseDocuments.AddRange(incomingDocs);

            // Save all PO changes first
            await _db.SaveChangesAsync(ct);

            // Recompute Indent POQty
            if (impacted.Count > 0)
                await RecomputeIndentPoQtyAsync(impacted, ct);

            await tx.CommitAsync(ct);
            return existing.Id;
        });
    }


    private static void ApplyScalarUpdates(PurchaseOrderHeader existing, PurchaseOrderHeader incoming)
    {
        existing.UnitId        = incoming.UnitId;
        existing.PODate        = incoming.PODate;
        existing.POCategoryId  = incoming.POCategoryId;
        existing.POMethodId    = incoming.POMethodId;
        existing.CurrencyId    = incoming.CurrencyId;
        existing.VendorId      = incoming.VendorId;

        existing.ItemTotal     = incoming.ItemTotal;
        existing.DiscountTotal = incoming.DiscountTotal;
        existing.PandFTotal    = incoming.PandFTotal;
        existing.MiscCharges   = incoming.MiscCharges;
        existing.GSTTotal      = incoming.GSTTotal;        
        existing.CGSTTotal     = incoming.CGSTTotal;
        existing.SGSTTotal     = incoming.SGSTTotal;
        existing.IGSTTotal     = incoming.IGSTTotal;
        existing.FreightTotal  = incoming.FreightTotal;
        existing.InsuranceTotal= incoming.InsuranceTotal;
        existing.TDSTotal      = incoming.TDSTotal;
        existing.AdvanceAmount = incoming.AdvanceAmount;
        existing.PurchaseValue = incoming.PurchaseValue;

        existing.StatusId      = incoming.StatusId;
        existing.RevisionNo    = incoming.RevisionNo;
        existing.AmendmentReason = incoming.AmendmentReason;
        existing.BudgetGroupId  = incoming.BudgetGroupId;
        // Optional FKs: 0 -> null normalization if not already handled
        existing.CapitalTypeId  = incoming.CapitalTypeId  > 0 ? incoming.CapitalTypeId  : null;
        existing.PurchaseTypeId = incoming.PurchaseTypeId > 0 ? incoming.PurchaseTypeId : null;
        existing.ProjectId      = incoming.ProjectId      > 0 ? incoming.ProjectId      : null;
        existing.WBSId      = incoming.WBSId      > 0 ? incoming.WBSId      : null;
        existing.FinancialYearId      = incoming.FinancialYearId      > 0 ? incoming.FinancialYearId      : null;
        existing.CostCenterId   = incoming.CostCenterId   > 0 ? incoming.CostCenterId   : null;
    }


    private static HashSet<int> GetImpactedIndentIdsFromPo(PurchaseOrderHeader po)
    {
        return po.Headers
           .SelectMany(h => h.Details)
            .Select(d => d.IndentId ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();
    }
    private async Task<HashSet<int>> GetImpactedIndentIdsFromDbAsync(int poId, CancellationToken ct)
    {
        var ids = await (
            from lh in _db.PurchaseLocalHeaders.AsNoTracking()
            where lh.PurchaseOrderId == poId
            join d in _db.PurchaseLocalDetails.AsNoTracking() on lh.Id equals d.PurchaseLocalId
            where d.IndentId != null && d.IndentId > 0
            select d.IndentId!.Value
        ).Distinct().ToListAsync(ct);

        return ids.ToHashSet();
    }
    public async Task RecomputeIndentPoQtyAsync(HashSet<int> indentHeaderIds, CancellationToken ct)
    {
        if (indentHeaderIds.Count == 0) return;

        // Sum quantities for the impacted indent headers across ALL active PO lines
        var sums = await _db.PurchaseLocalDetails
            .Where(d => d.IsDeleted == 0
                        && d.IndentId != null
                        && indentHeaderIds.Contains(d.IndentId.Value))
            .Join(_db.PurchaseLocalHeaders,
                  d => d.PurchaseLocalId,
                  lh => lh.Id,
                  (d, lh) => new { d, lh })
            .Where(x => x.lh.IsDeleted == 0)
            .Join(_db.PurchaseOrderHeaders,
                  x => x.lh.PurchaseOrderId,
                  h => h.Id,
                  (x, h) => new { x.d, h })
            .Where(y => y.h.IsDeleted == BaseEntity.IsDelete.NotDeleted)
            .GroupBy(y => new { IndentHeaderId = y.d.IndentId!.Value, y.d.ItemId })
            .Select(g => new
            {
                g.Key.IndentHeaderId,
                g.Key.ItemId,
                Qty = g.Sum(z => z.d.Quantity)
            })
            .ToListAsync(ct);

        // Fast lookup (IndentHeaderId, ItemId) -> Qty
        var qtyMap = sums.ToDictionary(k => (k.IndentHeaderId, k.ItemId), v => v.Qty);

        // Load indent detail rows we need to update
        var indentRows = await _db.IndentDetail
            .Where(r => r.IsDeleted == 0 && indentHeaderIds.Contains(r.IndentHeaderId))
            .ToListAsync(ct);

        foreach (var row in indentRows)
        {
            // Default to 0 if not found
            qtyMap.TryGetValue((row.IndentHeaderId, row.ItemId), out var totalQty);
            row.POQty = totalQty;
        }

        await _db.SaveChangesAsync(ct);
    }
    public async Task<int> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var hdr = await _db.PurchaseOrderHeaders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (hdr is null) return 0;

        hdr.IsDeleted = BaseEntity.IsDelete.Deleted;
        hdr.IsActive = BaseEntity.Status.Inactive;
        hdr.ModifiedDate = DateTimeOffset.UtcNow;

        return await _db.SaveChangesAsync(ct);
    }
   public async Task<string> GenerateNextCodeAsync(
    int poCategoryId,
    int? poMethodId,
    DateTimeOffset poDate,
    string unitCode,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(unitCode))
            throw new InvalidOperationException("Unit code is required. Failed to generate PO number.");

        // --- Get PO Category code (e.g., CAPITAL) ---
        var poCategoryCodeRaw = await _db.Set<MiscMaster>().AsNoTracking()
            .Where(m => m.Id == poCategoryId)
            .Select(m => m.Code)
            .FirstOrDefaultAsync(ct);

        var poCategoryCode = (poCategoryCodeRaw ?? string.Empty).Trim();

        // --- Get Method code (fallback NA) ---
        var methodCodeRaw = poMethodId is > 0
            ? await _db.Set<MiscMaster>().AsNoTracking()
                .Where(m => m.Id == poMethodId.Value)
                .Select(m => m.Code)
                .FirstOrDefaultAsync(ct)
            : "NA";

        var methodCode = (methodCodeRaw ?? "NA").Trim();

        
        if (poCategoryCode.Equals(MiscEnumEntity.Capital, StringComparison.OrdinalIgnoreCase))
            methodCode = MiscEnumEntity.Capital;

        var prefix = $"PO-{unitCode}-{methodCode}-";

        // ---- FY window (based on PODate) ---------------------------------------
        var (fyStart, fyEndEx) = GetFyRange(poDate, 4); // FY starts April

        // ---- compute max numeric suffix within FY & prefix ----------------------
        const string sql = @"
            ;WITH s AS (
                SELECT
                    TRY_CONVERT(int,
                        LEFT(
                            CASE 
                                WHEN CHARINDEX('-R', p.PONumber) > 0 
                                    THEN SUBSTRING(p.PONumber, LEN(@Prefix) + 1,
                                                CHARINDEX('-R', p.PONumber) - LEN(@Prefix) - 1)
                                ELSE SUBSTRING(p.PONumber, LEN(@Prefix) + 1, 50)
                            END,
                            PATINDEX('%[^0-9]%', 
                                    CASE 
                                        WHEN CHARINDEX('-R', p.PONumber) > 0 
                                            THEN SUBSTRING(p.PONumber, LEN(@Prefix) + 1,
                                                            CHARINDEX('-R', p.PONumber) - LEN(@Prefix) - 1)
                                        ELSE SUBSTRING(p.PONumber, LEN(@Prefix) + 1, 50)
                                    END + 'X') - 1
                        )
                    ) AS n
                FROM Purchase.PurchaseOrderHeader p
                WHERE p.PONumber LIKE @Prefix + '%'
                AND p.PODate >= @FyStart AND p.PODate < @FyEnd
            )
            SELECT ISNULL(MAX(n), 0)
            FROM s;";

        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

        var max = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { Prefix = prefix, FyStart = fyStart, FyEnd = fyEndEx },
                cancellationToken: ct
            )
        );

        var next = max + 1;
        return $"{prefix}{next:D2}";
    }

    private static (DateTimeOffset start, DateTimeOffset endExclusive) GetFyRange(DateTimeOffset poDate, int startMonth = 4)
    {
        TimeZoneInfo tz;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"); }
        catch { tz = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); }

        var y = poDate.Month >= startMonth ? poDate.Year : poDate.Year - 1;
        var localStart = new DateTime(y, startMonth, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var fyStart = new DateTimeOffset(localStart, tz.GetUtcOffset(localStart));
        return (fyStart, fyStart.AddYears(1));
    }
    public async Task<string> GetBaseDirectoryAsync(CancellationToken ct = default)
    {
        const string query = @"
            SELECT Description AS BaseDirectory
            FROM Purchase.MiscTypeMaster
            WHERE MiscTypeCode = 'POImage' AND IsDeleted = 0";
        var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(query);
        return result;
    }   
    public async Task<bool> UpdatePOApproveAsync(int poId, int statusId, CancellationToken ct = default)
    {
        var qch = await _db.PurchaseOrderHeaders
            .FirstOrDefaultAsync(h => h.Id == poId, ct);
        if (qch is null) return false;

        qch.StatusId = statusId;
        return await _db.SaveChangesAsync(ct) > 0;
    }

 public async Task<int> AmendAsync(
    PurchaseOrderHeader existing,
    PurchaseOrderUpdateDto dto,
    CancellationToken ct)
    {
        // 1) Resolve target status
        var pending = await _miscMasterQueryRepository
            .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

        // 2) Build next unique PO number
        var baseRoot = StripRevision(existing.PONumber);
        var nextPoNumber = string.IsNullOrWhiteSpace(dto.PONumber) ? baseRoot : dto.PONumber;
        var nextRevision = (existing.RevisionNo <= 0 ? 0 : existing.RevisionNo);

        while (await _db.PurchaseOrderHeaders.AsNoTracking()
            .AnyAsync(p => p.PONumber == nextPoNumber, ct))
        {
            nextRevision++;
            nextPoNumber = $"{baseRoot}-R{nextRevision}";
        }

        // 3) Compute impacted indents (old + new)
        var oldImpacted = await GetImpactedIndentIdsFromDbAsync(existing.Id, ct);
        var newImpacted = new HashSet<int>(
            (dto.Headers ?? Enumerable.Empty<PurchaseLocalHeaderDto>())
                .SelectMany(h => h.Details ?? Enumerable.Empty<PurchaseLocalDetailDto>())
                .Select(d => d.IndentId ?? 0)
                .Where(i => i > 0)
                .Distinct());
        var impacted = new HashSet<int>(oldImpacted);
        impacted.UnionWith(newImpacted);

        // 4) Build the REVISED root only with scalars; children will be inserted explicitly
        var revised = new PurchaseOrderHeader
        {
            // Root scalars from DTO
            UnitId        = dto.UnitId,
            PONumber      = nextPoNumber,
            PODate        = dto.PODate,
            POCategoryId  = dto.POCategoryId,
            POMethodId    = dto.POMethodId,
            CurrencyId    = dto.CurrencyId,
            VendorId      = dto.VendorId,
            ItemTotal     = dto.ItemTotal,
            DiscountTotal = dto.DiscountTotal,
            PandFTotal    = dto.PandFTotal,
            MiscCharges   = dto.MiscCharges,
            GSTTotal      = dto.GSTTotal,        
            CGSTTotal     = dto.CGSTTotal,
            SGSTTotal     = dto.SGSTTotal,
            IGSTTotal     = dto.IGSTTotal,
            FreightTotal  = dto.FreightTotal,
            InsuranceTotal= dto.InsuranceTotal,
            TDSTotal      = dto.TDSTotal,
            AdvanceAmount = dto.AdvanceAmount,
            PurchaseValue = dto.PurchaseValue,

            StatusId      = pending.Id,
            OldPOId       = existing.OldPOId ?? existing.Id,
            RevisionNo    = nextRevision,
            AmendmentReason = dto.AmendmentReason?.Trim(),

            CostCenterId  = dto.CostCenterId,
            ProjectId     = dto.ProjectId,
            WBSId         = dto.WBSId,
            CapitalTypeId = dto.CapitalTypeId,
            PurchaseTypeId= dto.PurchaseTypeId,
            BudgetGroupId = dto.BudgetGroupId,
            IsDeleted     = BaseEntity.IsDelete.NotDeleted,
            CreatedDate   = DateTimeOffset.UtcNow
        };

        var incomingDocs = (dto.Documents ?? new List<PurchaseDocumentDto>())
            .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
            
            .Select(d => new PurchaseDocument
            {
                // Id will be 0 for insert; PoId will be set after we know revised.Id
                DocumentId   = d.DocumentId,
                FileName     = d.FileName!,
                UploadedDate = d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
            })
            .ToList();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 6) Delete OLD local children/terms/docs
                var oldLocalHeaders = await _db.PurchaseLocalHeaders
                    .Where(h => h.PurchaseOrderId == existing.Id)
                    .Include(h => h.Details)
                    .ToListAsync(ct);

                if (oldLocalHeaders.Count > 0)
                {
                    var oldLocalDetails = oldLocalHeaders.SelectMany(h => h.Details).ToList();
                    if (oldLocalDetails.Count > 0) _db.PurchaseLocalDetails.RemoveRange(oldLocalDetails);
                    _db.PurchaseLocalHeaders.RemoveRange(oldLocalHeaders);
                }

                var oldTerms = await _db.PurchasePaymentTerms
                    .Where(t => t.PurchaseOrderId == existing.Id)
                    .ToListAsync(ct);
                if (oldTerms.Count > 0) _db.PurchasePaymentTerms.RemoveRange(oldTerms);

                var oldDocs = await _db.PurchaseDocuments
                    .Where(d => d.PoId == existing.Id)
                    .ToListAsync(ct);
                if (oldDocs.Count > 0) _db.PurchaseDocuments.RemoveRange(oldDocs);

                await _db.SaveChangesAsync(ct);

                // 7) Soft-close OLD root
                var current = await _db.PurchaseOrderHeaders
                    .FirstOrDefaultAsync(h => h.Id == existing.Id, ct)
                    ?? throw new InvalidOperationException($"PO {existing.Id} not found.");
                current.IsDeleted   = BaseEntity.IsDelete.Deleted;
                current.ModifiedDate= DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(ct);

                // 8) Insert REVISED root
                _db.PurchaseOrderHeaders.Add(revised);
                await _db.SaveChangesAsync(ct); // revised.Id available

                // 9) Insert REVISED children from DTO (headers -> details)
                foreach (var h in dto.Headers ?? Enumerable.Empty<PurchaseLocalHeaderDto>())
                {
                    var hEntity = new PurchaseLocalHeader
                    {
                        PurchaseOrderId         = revised.Id,
                        IsPartialReceiptAllowed = h.IsPartialReceiptAllowed,
                        IncotermsId             = h.IncotermsId,
                        ModeOfDispatchId        = h.ModeOfDispatchId,
                        FreightCharges          = h.FreightCharges,
                        TermsId                 = h.TermsId,
                        TermDescription         = h.TermDescription,
                        DeliveryAddress         = h.DeliveryAddress,
                        BillingAddress          = h.BillingAddress,
                        Details = (h.Details ?? new List<PurchaseLocalDetailDto>()).Select(d => new PurchaseLocalDetail
                        {
                            IndentId        = d.IndentId,
                            ItemId          = d.ItemId,
                            ItemSno         = d.ItemSno,
                            UOMId           = d.UOMId,
                            Quantity        = d.Quantity,
                            UnitPrice       = d.UnitPrice,
                            LastPOPrice     = d.LastPOPrice,
                            DiscountTypeId  = d.DiscountTypeId,
                            DiscountValue   = d.DiscountValue,
                            PandFType       = d.PandFType,
                            PandFCharge     = d.PandFCharge,
                            OtherCharge     = d.OtherCharge,
                            GSTPercentage   = d.GSTPercentage,
                            CGST            = d.CGST,
                            SGST            = d.SGST,
                            IGST            = d.IGST,
                            CGSTPercentage  = (d.IGST > 0) ? d.GSTPercentage / 2 : 0,
                            SGSTPercentage  = (d.IGST > 0) ? d.GSTPercentage / 2 : 0,
                            IGSTPercentage  = (d.IGST > 0) ? 0 : d.GSTPercentage,
                            ScheduleDate    = d.ScheduleDate,
                            DepartmentId    = d.DepartmentId,
                            ItemValue       = d.ItemValue
                        }).ToList()
                    };
                    _db.PurchaseLocalHeaders.Add(hEntity);
                }

                foreach (var t in dto.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTermDto>())
                {
                    _db.PurchasePaymentTerms.Add(new PurchasePaymentTerm
                    {
                        PurchaseOrderId = revised.Id,
                        PaymentTermId   = t.PaymentTermId,
                        AdvancePercent  = t.AdvancePercent,
                        CreditDays      = t.CreditDays,
                        PaymentModelId  = t.PaymentModelId,
                        InsuranceId     = t.InsuranceId,
                        InsurancePercent= t.InsurancePercent,
                        InsuranceAmount = t.InsuranceAmount,
                        AdvanceAmount   = t.AdvanceAmount,
                        BalancePercent  = t.BalancePercent,
                        BalanceAmount   = t.BalanceAmount
                    });
                }

                // 10) Insert REVISED documents EXACTLY as payload
                if (incomingDocs.Count > 0)
                {
                    foreach (var d in incomingDocs)
                    {
                        d.Id  = 0;
                        d.PoId= revised.Id;
                        if (d.UploadedDate == default) d.UploadedDate = DateTimeOffset.UtcNow;
                    }
                    _db.PurchaseDocuments.AddRange(incomingDocs);
                }

                await _db.SaveChangesAsync(ct);

                // 11) Recompute impacted indents
                if (impacted.Count > 0)
                    await RecomputeIndentPoQtyAsync(impacted, ct);

                await tx.CommitAsync(ct);
                return revised.Id;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    private static string StripRevision(string code)
    {
        var idx = code.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0 && idx + 2 < code.Length && int.TryParse(code[(idx + 2)..], out _))
            return code[..idx];
        return code;
    }
    public Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct)
        => _db.PurchaseOrderHeaders
              .Include(h => h.Headers).ThenInclude(l => l.Details)
              .Include(h => h.PaymentTerms)
              .FirstOrDefaultAsync(h => h.Id == id, ct);

}
