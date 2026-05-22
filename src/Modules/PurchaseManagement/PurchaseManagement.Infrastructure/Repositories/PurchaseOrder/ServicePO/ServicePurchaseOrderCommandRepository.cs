#nullable disable
using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ServicePO
{
    public class ServicePurchaseOrderCommandRepository : IServicePurchaseOrderCommandRepository
    {
        private readonly ApplicationDbContext _db;

        private readonly IMiscMasterQueryRepository _misc;
        private readonly IIPAddressService _ip;
        private readonly IDbConnection _dbConnection;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public ServicePurchaseOrderCommandRepository(ApplicationDbContext db, IMiscMasterQueryRepository misc, IIPAddressService ip, IDbConnection dbConnection, IDocumentSequenceLookup documentSequenceLookup)
        {
            _db = db;
            _misc = misc;
            _ip = ip;
            _dbConnection = dbConnection;
            _documentSequenceLookup = documentSequenceLookup;
        }
        public async Task<PurchaseOrderHeader> GetAggregateAsync(int id, CancellationToken ct)
        {
            // Tracked entity (needed for soft-close + EF graph insert in AmendAsync)
            return await _db.PurchaseOrderHeaders
                .AsSplitQuery() // avoids cartesian explosion
                .Include(p => p.ServicePos)
                    .ThenInclude(h => h.Items)
                        .ThenInclude(l => l.PurchaseOrderServiceSchedules)
                .Include(p => p.PaymentTerms)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);
        }

        public async Task<int> AmendAsync(
             PurchaseOrderHeader existing,
             PurchaseOrderHeader revised,
             CancellationToken ct)
        {
            // ---------- build new PONumber & revision ----------
            var baseRoot = StripRevision(existing.PONumber);

            if (revised.RevisionNo <= existing.RevisionNo)
                revised.RevisionNo = existing.RevisionNo + 1;

            revised.PONumber = $"{baseRoot}-R{revised.RevisionNo}";
            while (await _db.PurchaseOrderHeaders.AsNoTracking()
                   .AnyAsync(p => p.PONumber == revised.PONumber, ct))
            {
                revised.RevisionNo++;
                revised.PONumber = $"{baseRoot}-R{revised.RevisionNo}";
            }

            revised.UnitId = existing.UnitId;
            revised.OldPOId = existing.OldPOId ?? existing.Id;

            // Status => Pending
            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            revised.StatusId = pending.Id;

            // Stamps
            revised.CreatedBy = _ip.GetUserId();
            revised.CreatedByName = _ip.GetUserName();
            revised.CreatedIP = _ip.GetSystemIPAddress();
            revised.CreatedDate = DateTimeOffset.UtcNow;
            revised.IsDeleted = BaseEntity.IsDelete.NotDeleted;

            // ---------- Phase 0: lift schedules out (need real ids later) ----------
            var pendingSchedules = ExtractAndDetachSchedules(revised);

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    // 1) Soft close old
                    var current = await _db.PurchaseOrderHeaders
                        .FirstOrDefaultAsync(h => h.Id == existing.Id, ct)
                        ?? throw new InvalidOperationException($"PO {existing.Id} not found.");

                    current.IsDeleted = BaseEntity.IsDelete.Deleted;
                    current.ModifiedDate = DateTimeOffset.UtcNow;
                    await _db.SaveChangesAsync(ct);

                    // Clear tracker to avoid duplicate tracking conflicts
                    _db.ChangeTracker.Clear();

                    // 2) Zero all Ids and add revised (no schedules yet)
                    ZeroIdsForInsert(revised);

                    _db.PurchaseOrderHeaders.Add(revised);
                    await _db.SaveChangesAsync(ct); // assigns header, headers, lines ids

                    var newPoId = revised.Id;

                    // Ensure FKs for header/lines
                    foreach (var sh in revised.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeader>())
                    {
                        sh.PurchaseOrderId = newPoId;
                        foreach (var ln in sh.Items ?? Enumerable.Empty<PurchaseOrderServiceLine>())
                        {
                            ln.PurchaseOrderId = newPoId;
                            ln.ServicePoHeaderId = sh.Id;
                        }
                    }
                    await _db.SaveChangesAsync(ct);

                    // 3) Phase 2: insert schedules with real IDs
                    if (pendingSchedules.Count > 0)
                    {
                        foreach (var item in pendingSchedules)
                        {
                            // resolve REAL line & header by client temp keys we stored
                            var line = item.LineRef;
                            var sched = item.Schedule;

                            // attach with actual ids on the inserted graph
                            sched.Id = 0;
                            sched.PurchaseOrderId = newPoId;
                            sched.ServicePoHeaderId = line.ServicePoHeaderId;
                            sched.ServiceItemId = line.Id;

                            _db.PurchaseOrderServiceSchedule.Add(sched);
                        }

                        await _db.SaveChangesAsync(ct);
                    }

                    await tx.CommitAsync(ct);
                    return newPoId;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }


        private static List<(PurchaseOrderServiceLine LineRef, PurchaseOrderServiceSchedule Schedule)> ExtractAndDetachSchedules(PurchaseOrderHeader root)
        {
            var list = new List<(PurchaseOrderServiceLine, PurchaseOrderServiceSchedule)>();

            foreach (var sh in root.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeader>())
            {
                foreach (var ln in sh.Items ?? Enumerable.Empty<PurchaseOrderServiceLine>())
                {
                    if (ln.PurchaseOrderServiceSchedules?.Count > 0)
                    {
                        foreach (var sc in ln.PurchaseOrderServiceSchedules)
                            list.Add((ln, sc));

                        // Clear schedules before first SaveChanges to avoid FK problems
                        ln.PurchaseOrderServiceSchedules = new List<PurchaseOrderServiceSchedule>();
                    }
                }
            }
            return list;
        }

        private static string StripRevision(string po)
        {
            var idx = po.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0 && idx + 2 < po.Length && int.TryParse(po[(idx + 2)..], out _))
                return po[..idx];
            return po;
        }
        private static void ZeroIdsForInsert(PurchaseOrderHeader root)
        {
            root.Id = 0;

            foreach (var sh in root.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeader>())
            {
                sh.Id = 0;
                sh.PurchaseOrderId = 0;

                foreach (var ln in sh.Items ?? Enumerable.Empty<PurchaseOrderServiceLine>())
                {
                    ln.Id = 0;
                    ln.PurchaseOrderId = 0;
                    ln.ServicePoHeaderId = 0;

                    foreach (var sc in ln.PurchaseOrderServiceSchedules ?? Enumerable.Empty<PurchaseOrderServiceSchedule>())
                    {
                        sc.Id = 0;
                        sc.PurchaseOrderId = 0;
                        sc.ServicePoHeaderId = 0;
                        sc.ServiceItemId = 0;
                    }
                }
            }

            foreach (var t in root.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTerm>())
            {
                t.Id = 0;
                t.PurchaseOrderId = 0;
            }
        }

        public async Task<int> CreateAsync(PurchaseOrderHeader aggregate, CancellationToken ct, int? transactionTypeId = null)
        {
            // 0) status = Pending
            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            aggregate.StatusId = pending.Id;

            // Lookups for ServiceOrderType
            var oneTime = await _misc.GetMiscMasterByName("ServiceOrderType", "OneTime");
            var recurring = await _misc.GetMiscMasterByName("ServiceOrderType", "Recurring");

            var pendingSchedules = new List<(PurchaseOrderServiceLine line, PurchaseOrderServiceSchedule sched)>();

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // A) Walk only SERVICE PO graph
                foreach (var sh in aggregate.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeader>())
                {
                    sh.Id = 0;

                    // Normalize 0 -> null to avoid FK violations
                    if (sh.ContractTypeId == 0) sh.ContractTypeId = null;
                    if (sh.FrequencyId == 0) sh.FrequencyId = null;

                    var isOneTime = sh.ServiceCategoryId == oneTime.Id;
                    var isRecurring = sh.ServiceCategoryId == recurring.Id;

                    if (isOneTime)
                    {
                        // OneTime => clear all recurring-only fields
                        sh.ContractTypeId = null;
                        sh.FrequencyId = null;
                        sh.ValidityFrom = null;
                        sh.ValidityTo = null;
                        sh.TotalOccurrences = null;
                        sh.OverallLimit = null;

                    }
                    else if (isRecurring)
                    {
                        // Basic validation for Recurring
                        if (sh.FrequencyId == null || sh.ValidityFrom == null || sh.ValidityTo == null)
                            throw new InvalidOperationException("Recurring Service PO requires FrequencyId, ValidityFrom, and ValidityTo.");
                    }

                    foreach (var ln in sh.Items ?? Enumerable.Empty<PurchaseOrderServiceLine>())
                    {
                        ln.Id = 0;
                        ln.PurchaseOrderId = 0;
                        ln.ServicePoHeaderId = 0;

                        // Collect schedules only for Recurring, never for OneTime
                        if (isRecurring)
                        {
                            foreach (var sc in ln.PurchaseOrderServiceSchedules ?? Enumerable.Empty<PurchaseOrderServiceSchedule>())
                            {
                                sc.Id = 0;
                                pendingSchedules.Add((ln, sc));
                            }
                        }

                        // Always detach schedules before first SaveChanges, so EF doesn't insert early
                        ln.PurchaseOrderServiceSchedules = new List<PurchaseOrderServiceSchedule>();
                    }
                }

                // Payment terms (allowed for service PO)
                foreach (var t in aggregate.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTerm>())
                {
                    t.Id = 0;
                    t.PurchaseOrderId = 0;
                }

                // B) First save: header + service headers + lines
                _db.PurchaseOrderHeaders.Add(aggregate);
                await _db.SaveChangesAsync(ct);
                var poId = aggregate.Id;

                // Fix FKs after first save
                foreach (var sh in aggregate.ServicePos ?? Enumerable.Empty<PurchaseOrderServiceHeader>())
                {
                    sh.PurchaseOrderId = poId;
                    foreach (var ln in sh.Items ?? Enumerable.Empty<PurchaseOrderServiceLine>())
                    {
                        ln.PurchaseOrderId = poId;
                        ln.ServicePoHeaderId = sh.Id;
                    }
                }

                // C) Second pass: insert schedules with real IDs (only if any; OneTime adds none)
                if (pendingSchedules.Count > 0)
                {
                    foreach (var (line, sched) in pendingSchedules)
                    {
                        sched.PurchaseOrderId = poId;
                        sched.ServicePoHeaderId = line.ServicePoHeaderId;
                        sched.ServiceItemId = line.Id;

                        _db.PurchaseOrderServiceSchedule.Add(sched);
                    }
                }

                await _db.SaveChangesAsync(ct);

                // Advance Finance.DocumentSequence.DocNo for this PO's TransactionType — same
                // connection + transaction as the inserts above so the increment is atomic
                // with the PO save (mirrors Local PO numbering). Skipped when caller doesn't
                // use IDocumentSequenceLookup-based numbering (transactionTypeId == null).
                if (transactionTypeId is > 0)
                {
                    var dbConn = _db.Database.GetDbConnection();
                    var dbTx = tx.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId.Value, dbConn, dbTx);
                }

                await tx.CommitAsync(ct);
                return poId;
            });
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


     
        public async Task<ServiceEntrySheet> CreateServiceEntrySheetAsync(ServiceEntrySheet aggregate, CancellationToken ct = default)
        {

            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            aggregate.StatusId = pending.Id;

            aggregate.Activities ??= new List<ServiceEntryActivity>();

            await _db.ServiceEntrySheets.AddAsync(aggregate, ct);
            await _db.SaveChangesAsync(ct);

            return aggregate;
        }

        public async Task<bool> ServiceEntrySheetExistsAsync(    int purchaseOrderId,    int? serviceScheduleId,    CancellationToken ct = default)
            {
                return await _db.ServiceEntrySheets 
                    .AnyAsync(x =>   x.PurchaseOrderId == purchaseOrderId &&  x.ScheduleId == serviceScheduleId,      ct);
            }

        public async Task<bool> UpdateAsync(PurchaseOrderHeader aggregate, CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var dbPo = await _db.PurchaseOrderHeaders
                    .AsSplitQuery()
                    .Include(p => p.ServicePos)
                        .ThenInclude(h => h.Items)
                            .ThenInclude(l => l.PurchaseOrderServiceSchedules)
                    .Include(p => p.PaymentTerms)
                    .FirstOrDefaultAsync(p => p.Id == aggregate.Id && p.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

                if (dbPo == null) return false;

                // --- scalar header fields you allow to change ---
                dbPo.PODate = aggregate.PODate;
                dbPo.POCategoryId = aggregate.POCategoryId;
                dbPo.POMethodId = aggregate.POMethodId;
                dbPo.CurrencyId = aggregate.CurrencyId;
                dbPo.VendorId = aggregate.VendorId;
                dbPo.ItemTotal = aggregate.ItemTotal;
                dbPo.DiscountTotal = aggregate.DiscountTotal;
                dbPo.PandFTotal = aggregate.PandFTotal;
                dbPo.MiscCharges = aggregate.MiscCharges;
                dbPo.GSTTotal = aggregate.GSTTotal;
                dbPo.CGSTTotal = aggregate.CGSTTotal;
                dbPo.SGSTTotal = aggregate.SGSTTotal;
                dbPo.IGSTTotal = aggregate.IGSTTotal;
                dbPo.FreightTotal = aggregate.FreightTotal;
                dbPo.InsuranceTotal = aggregate.InsuranceTotal;
                dbPo.TDSTotal = aggregate.TDSTotal;
                dbPo.AdvanceAmount = aggregate.AdvanceAmount;
                dbPo.PurchaseValue = aggregate.PurchaseValue;
                dbPo.AmendmentReason = aggregate.AmendmentReason;

                // --- payment terms: wipe & re-add (FK-safe) ---
                _db.PurchasePaymentTerms.RemoveRange(dbPo.PaymentTerms);
                dbPo.PaymentTerms.Clear();
                foreach (var t in aggregate.PaymentTerms)
                {
                    t.Id = 0;
                    t.PurchaseOrderId = dbPo.Id;
                    dbPo.PaymentTerms.Add(t);
                }

                // Resolve OneTime/Recurring ids (no hard-coding)
                var oneTimeId = (await _misc.GetMiscMasterByName("ServiceOrderType", "OneTime")).Id;
                var recurringId = (await _misc.GetMiscMasterByName("ServiceOrderType", "Recurring")).Id;

                // --- headers: delete missing (and their children, FK-safe) ---
                var incomingHeaderIds = aggregate.ServicePos.Where(h => h.Id > 0).Select(h => h.Id).ToHashSet();
                var toRemoveHeaders = dbPo.ServicePos.Where(h => !incomingHeaderIds.Contains(h.Id)).ToList();
                foreach (var rem in toRemoveHeaders)
                {
                    foreach (var remLine in rem.Items)
                        _db.PurchaseOrderServiceSchedule.RemoveRange(remLine.PurchaseOrderServiceSchedules);
                    _db.PurchaseOrderServiceLine.RemoveRange(rem.Items);
                    _db.PurchaseOrderServiceHeader.Remove(rem);
                }

                // --- upsert headers ---
                foreach (var incHeader in aggregate.ServicePos)
                {
                    // Normalize OneTime vs Recurring
                    var isOneTime = incHeader.ServiceCategoryId == oneTimeId;

                    if (isOneTime)
                    {
                        // For OneTime, nullify specific fields
                        incHeader.ContractTypeId = null;
                        incHeader.FrequencyId = null;
                        incHeader.ValidityFrom = null;
                        incHeader.ValidityTo = null;
                        incHeader.TotalOccurrences = null;
                        incHeader.OverallLimit = null;                       

                        // Ensure schedules are empty for OneTime
                        foreach (var line in incHeader.Items)
                            line.PurchaseOrderServiceSchedules.Clear();  // Must be empty for OneTime
                    }
                    else
                    {
                        // Recurring: Ensure necessary fields are populated
                        // No need to nullify fields, they should be populated based on incoming data
                    }

                    if (incHeader.Id == 0)
                    {
                        // new header
                        incHeader.PurchaseOrderId = dbPo.Id;

                        // Ensure children are new
                        foreach (var ln in incHeader.Items)
                        {
                            ln.Id = 0;
                            ln.PurchaseOrderId = dbPo.Id;
                            ln.ServicePoHeaderId = 0; // EF will set after add
                            foreach (var sc in ln.PurchaseOrderServiceSchedules)
                                sc.Id = 0;
                        }

                        // Add new header
                        dbPo.ServicePos.Add(incHeader);

                        // If there are schedules, we need line ids first
                        await _db.SaveChangesAsync(ct);

                        // Attach schedules for each new line with real ids
                        foreach (var ln in incHeader.Items)
                        {
                            foreach (var sc in ln.PurchaseOrderServiceSchedules)
                            {
                                sc.Id = 0;
                                sc.PurchaseOrderId = dbPo.Id;
                                sc.ServicePoHeaderId = incHeader.Id;
                                sc.ServiceItemId = ln.Id;
                                _db.PurchaseOrderServiceSchedule.Add(sc);
                            }
                        }
                    }
                    else
                    {
                        // Existing header: Find and update
                        var dbHeader = dbPo.ServicePos.First(h => h.Id == incHeader.Id);

                        dbHeader.ServiceCategoryId = incHeader.ServiceCategoryId;
                        dbHeader.ContractTypeId = incHeader.ContractTypeId;
                        dbHeader.FrequencyId = incHeader.FrequencyId;
                        dbHeader.ValidityFrom = incHeader.ValidityFrom;
                        dbHeader.ValidityTo = incHeader.ValidityTo;
                        dbHeader.TotalOccurrences = incHeader.TotalOccurrences;
                        dbHeader.OverallLimit = incHeader.OverallLimit;
                        dbHeader.TermsId = incHeader.TermsId;
                        dbHeader.CostCenterId = incHeader.CostCenterId;
                        dbHeader.ModeOfDispatchId = incHeader.ModeOfDispatchId;
                        dbHeader.FreightCharges = incHeader.FreightCharges;
                        dbHeader.TermDescription = incHeader.TermDescription;
                        dbHeader.DeliveryAddress = incHeader.DeliveryAddress;
                        dbHeader.BillingAddress = incHeader.BillingAddress;
                        dbHeader.POImage = incHeader.POImage;

                        // Lines: Delete missing (and their schedules first)
                        var incomingLineIds = incHeader.Items.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();
                        var toRemoveLines = dbHeader.Items.Where(l => !incomingLineIds.Contains(l.Id)).ToList();
                        foreach (var remLine in toRemoveLines)
                        {
                            _db.PurchaseOrderServiceSchedule.RemoveRange(remLine.PurchaseOrderServiceSchedules);
                            _db.PurchaseOrderServiceLine.Remove(remLine);
                        }

                        // Upsert lines
                        foreach (var incLine in incHeader.Items)
                        {
                            if (incLine.Id == 0)
                            {
                                incLine.PurchaseOrderId = dbPo.Id;
                                incLine.ServicePoHeaderId = dbHeader.Id;

                                var scheds = incLine.PurchaseOrderServiceSchedules.ToList();
                                incLine.PurchaseOrderServiceSchedules = new List<PurchaseOrderServiceSchedule>();
                                dbHeader.Items.Add(incLine);

                                await _db.SaveChangesAsync(ct);

                                foreach (var sc in scheds)
                                {
                                    sc.Id = 0;
                                    sc.PurchaseOrderId = dbPo.Id;
                                    sc.ServicePoHeaderId = dbHeader.Id;
                                    sc.ServiceItemId = incLine.Id;
                                    _db.PurchaseOrderServiceSchedule.Add(sc);
                                }
                            }
                            else
                            {
                                var dbLine = dbHeader.Items.First(l => l.Id == incLine.Id);

                                dbLine.LineNo = incLine.LineNo;
                                dbLine.RequestId = incLine.RequestId;
                                dbLine.ServiceId = incLine.ServiceId;
                                dbLine.ServiceDescription = incLine.ServiceDescription;
                                dbLine.UOMId = incLine.UOMId;
                                dbLine.PlannedQuantity = incLine.PlannedQuantity;
                                dbLine.PlannedRate = incLine.PlannedRate;
                                dbLine.DiscountId = incLine.DiscountId;
                                dbLine.Discount = incLine.Discount;
                                dbLine.ItemCost = incLine.ItemCost;
                                dbLine.OtherCost = incLine.OtherCost;
                                dbLine.OtherCharges = incLine.OtherCharges;
                                dbLine.GstPercent = incLine.GstPercent;
                                dbLine.Remarks = incLine.Remarks;

                                // Replace schedules (FK-safe)
                                _db.PurchaseOrderServiceSchedule.RemoveRange(dbLine.PurchaseOrderServiceSchedules);
                                foreach (var sc in incLine.PurchaseOrderServiceSchedules)
                                {
                                    sc.Id = 0;
                                    sc.PurchaseOrderId = dbPo.Id;
                                    sc.ServicePoHeaderId = dbHeader.Id;
                                    sc.ServiceItemId = dbLine.Id;
                                    _db.PurchaseOrderServiceSchedule.Add(sc);
                                }
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return true;
            });
        }

        public async Task<bool> UpdateServicePOApproveAsync(int poId, int statusId, CancellationToken ct = default)
        {
            var po = await _db.PurchaseOrderHeaders
                .FirstOrDefaultAsync(h => h.Id == poId, ct);
            if (po is null) return false;
            if (po.StatusId == statusId) return false;

            po.StatusId = statusId;
            return await _db.SaveChangesAsync(ct) > 0;
        }


        public async Task<ServiceEntrySheet> UpdateServiceEntrySheetAsync(
          ServiceEntrySheet aggregate,
          CancellationToken ct = default)
        {

            // EF is already tracking 'aggregate' if loaded via context
            _db.ServiceEntrySheets.Update(aggregate);
            await _db.SaveChangesAsync(ct);
            return aggregate;
        }

        // 🔹 NEW: Load SES by Id including Activities
        public async Task<ServiceEntrySheet> GetServiceEntrySheetByIdAsync(int id, CancellationToken ct = default)
        {
                return await _db.ServiceEntrySheets
                    .Include(s => s.Activities)
                    .FirstOrDefaultAsync(s => s.Id == id, ct);
        }
        
        public async Task<bool> UpdateServiceEntrySheetApproveAsync(
        int Id,
        int StatusId,       
        CancellationToken ct = default)
        {
            // Load existing SES
            var ses = await _db.ServiceEntrySheets
                .FirstOrDefaultAsync(x => x.Id == Id , ct);

            if (ses is null)
                return false;
           
            ses.StatusId = StatusId;        

            _db.ServiceEntrySheets.Update(ses);
            await _db.SaveChangesAsync(ct);  
            return true;
        }


       

    }
}