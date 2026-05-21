using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using PurchaseManagement.Domain.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ImportPO
{
    public class ImportPOCommandRepository : IImportPOCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly ILogger<ImportPOCommandRepository> _logger;
        private readonly IIPAddressService _ipAddressService;

        public ImportPOCommandRepository(
            ApplicationDbContext db,
            IMiscMasterQueryRepository misc,
            ILogger<ImportPOCommandRepository> logger,
            IIPAddressService ipAddressService)
        {
            _db = db;
            _misc = misc;
            _logger = logger;
            _ipAddressService = ipAddressService;
        }

        /* ========================= CREATE ========================= */
        public async Task<int> CreateAsync(PurchaseOrderHeader aggregate, ImportPOCreateDto dto, CancellationToken ct)
        {
            foreach (var h in aggregate.ImportPOHeader ?? Enumerable.Empty<ImportPOHeader>())
            {
                h.Id = 0;
                foreach (var d in h.ImportPODetails ?? Enumerable.Empty<ImportPODetail>())
                    d.Id = 0;
            }
            foreach (var t in aggregate.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTerm>())
                t.Id = 0;
            foreach (var doc in aggregate.PurchaseDocumentTypes ?? Enumerable.Empty<PurchaseDocument>())
                doc.Id = 0;

            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            aggregate.StatusId = pending.Id;

            var impacted = aggregate.ImportPOHeader?
                .SelectMany(h => h.ImportPODetails ?? [])
                .Select(d => d.IndentId ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToHashSet() ?? new HashSet<int>();

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                _db.PurchaseOrderHeaders.Add(aggregate);
                await _db.SaveChangesAsync(ct); // PK ready

                // Fallback: if handler didn’t attach docs to entity, insert from dto
                if ((aggregate.PurchaseDocumentTypes == null || aggregate.PurchaseDocumentTypes.Count == 0)
                    && dto.Documents != null && dto.Documents.Count > 0)
                {
                    var docs = dto.Documents
                        .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
                        .Select(d => new PurchaseDocument
                        {
                            PoId = aggregate.Id,
                            DocumentId = d.DocumentId,
                            FileName = d.FileName,
                            UploadedDate = d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
                        })
                        .ToList();

                    if (docs.Count > 0)
                    {
                        _db.PurchaseDocuments.AddRange(docs);
                        await _db.SaveChangesAsync(ct);
                    }
                }
                else
                {
                    // Ensure FK for docs attached on aggregate (if any)
                    if (aggregate.PurchaseDocumentTypes != null && aggregate.PurchaseDocumentTypes.Count > 0)
                    {
                        foreach (var doc in aggregate.PurchaseDocumentTypes)
                            doc.PoId = aggregate.Id;
                        await _db.SaveChangesAsync(ct);
                    }
                }

                if (impacted.Count > 0)
                    await RecomputeImportIndentPoQtyAsync(impacted, ct);
                
                await tx.CommitAsync(ct);
                return aggregate.Id;
            });
        }

        public async Task<List<int>> GetPODocumentIdsAsync(int poId)
        {
            return await _db.PurchaseDocuments
                .Where(d => d.PoId == poId)
                .Select(d => d.DocumentId)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(PurchaseOrderHeader incoming, ImportPOUpdateDto dto, CancellationToken ct)
        {
            if (dto == null || dto.Id <= 0) return 0;

            var existing = await _db.PurchaseOrderHeaders
                .Include(x => x.ImportPOHeader).ThenInclude(h => h.ImportPODetails)
                .Include(x => x.PaymentTerms)
                .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

            if (existing is null) return 0;

            // ✅ Capture BEFORE applying scalar updates (because existing values will be overwritten)
            var oldBudgetGroupId = existing.BudgetGroupId;
            var oldPoDate        = existing.PODate;
            var oldRequestById      = existing.BudgetRequestById;
            var oldMonthId      = existing.BudgetMonthId;
            var oldValue         = existing.PurchaseValue;
            var oldProjectId        = existing.ProjectId;
            var oldWbsId         = existing.WBSId;
            var oldFinancialYearId = existing.FinancialYearId;

            var newBudgetGroupId = incoming.BudgetGroupId;
            var newPoDate        = incoming.PODate;                            
            var newRequestById      = incoming.BudgetRequestById;            
            var newMonthId      = incoming.BudgetMonthId;
            var newValue         = incoming.PurchaseValue;
            var newProjectId        = incoming.ProjectId;
            var newWbsId         = incoming.WBSId;
            var newFinancialYearId = incoming.FinancialYearId;

            incoming.StatusId = existing.StatusId;

            // ---- impacted (old + new) ----
            var oldIndents = (existing.ImportPOHeader ?? new List<ImportPOHeader>())
                .SelectMany(h => h.ImportPODetails ?? new List<ImportPODetail>())
                .Select(d => d.IndentId.GetValueOrDefault())
                .Where(i => i > 0).Distinct().ToHashSet();

            var newIndents = (dto.Headers ?? new List<ImportPOHeaderDto>())
                .SelectMany(h => h.Details ?? new List<ImportPODetailDto>())
                .Select(d => d.IndentId)
                .Where(i => i > 0).Distinct().ToHashSet();

            var impacted = new HashSet<int>(oldIndents);
            impacted.UnionWith(newIndents);

            // ---- documents (single source: dto.Documents) ----
            var incomingDocs = (dto.Documents ?? new List<DocumentDto>())
                .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
                .GroupBy(d => d.DocumentId)                  // keep one per DocumentId
                .Select(g => g.First())
                .Select(d => new PurchaseDocument
                {
                    // DO NOT set Id (identity)
                    PoId = existing.Id,
                    DocumentId = d.DocumentId,
                    FileName = d.FileName!,
                    UploadedDate = d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
                })
                .ToList();

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // 1) Update scalar fields explicitly
                ApplyScalarUpdates(existing, incoming);
                existing.ModifiedDate = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(ct);

                // 2) Remove children (details -> headers), terms, and existing documents
                if (existing.ImportPOHeader?.Count > 0)
                {
                    var oldDetails = existing.ImportPOHeader
                        .SelectMany(h => h.ImportPODetails ?? new List<ImportPODetail>())
                        .ToList();

                    if (oldDetails.Count > 0)
                        _db.ImportPODetail.RemoveRange(oldDetails);

                    _db.ImportPOHeader.RemoveRange(existing.ImportPOHeader);
                }

                if (existing.PaymentTerms?.Count > 0)
                    _db.PurchasePaymentTerms.RemoveRange(existing.PaymentTerms);

                var oldDocs = await _db.PurchaseDocuments
                    .Where(d => d.PoId == existing.Id)
                    .ToListAsync(ct);

                if (oldDocs.Count > 0)
                    _db.PurchaseDocuments.RemoveRange(oldDocs);

                await _db.SaveChangesAsync(ct);

                // 3) Rebuild from DTO (headers -> details)
                foreach (var h in dto.Headers ?? Enumerable.Empty<ImportPOHeaderDto>())
                {
                    var hEntity = new ImportPOHeader
                    {
                        PurchaseOrderId = existing.Id,
                        TTExchangeRateId = h.TTExchangeRateId,
                        TTExchangeRate = h.TTExchangeRate,
                        IncotermId = h.IncotermId,
                        ShippingPortId = h.ShippingPortId,
                        DestinationPortId = h.DestinationPortId,
                        ModeOfTransportId = h.ModeOfTransportId,
                        ShippingModeId = h.ShippingModeId,
                        CustomsOfficeId = h.CustomsOfficeId,
                        OriginCountryId = h.OriginCountryId,
                        InsuranceProviderId = h.InsuranceProviderId,
                        FreightForwarderId = h.FreightForwarderId,
                        FreeDaysAllowed = h.FreeDaysAllowed,
                        DemurrageTerms = h.DemurrageTerms,
                        BillOfLadingNumber = h.BillOfLadingNumber,
                        VesselName = h.VesselName,
                        ContainerNumber = h.ContainerNumber,
                        AirlineName = h.AirlineName,
                        AirWaybillNumber = h.AirWaybillNumber,
                        AirWaybillDate = h.AirWaybillDate,
                        FlightNumber = h.FlightNumber,
                        ExpectedTimeOfDeparture = h.ExpectedTimeOfDeparture,
                        ExpectedTimeOfArrival = h.ExpectedTimeOfArrival,
                        CustomsHouseAgentId = h.CustomsHouseAgentId,
                        BillOfEntryNumber = h.BillOfEntryNumber,
                        LCPaymentModeId = h.LCPaymentModeId,
                        LCPaymentStatusId = h.LCPaymentStatusId,
                        LCNumber = h.LCNumber,
                        LCDate = h.LCDate,
                        LCExpiryDate = h.LCExpiryDate,
                        LCCurrencyId = h.LCCurrencyId,
                        LCAmount = h.LCAmount,
                        LCIssueBankId = h.LCIssueBankId,
                        LCBeneficiaryBankId = h.LCBeneficiaryBankId,
                        LCTypeId = h.LCTypeId,
                        LCRemarks = h.LCRemarks,
                        TTReferenceNumber = h.TTReferenceNumber,
                        TTTransferDate = h.TTTransferDate,
                        TTBankId = h.TTBankId,
                        TTCurrencyId = h.TTCurrencyId,
                        TTPaymentModeId = h.TTPaymentModeId,
                        TTPaymentStatusId = h.TTPaymentStatusId,
                        TTRemarks = h.TTRemarks,
                        LCSwiftCode = h.LCSwiftCode,
                        TTSwiftCode = h.TTSwiftCode,
                        IsPartialReceiptAllowed = h.IsPartialReceiptAllowed,
                        ImportPODetails = (h.Details ?? new List<ImportPODetailDto>()).Select(d => new ImportPODetail
                        {
                            IndentId = d.IndentId,
                            ItemId = d.ItemId,
                            ItemSno = d.ItemSno,
                            Quantity = d.Quantity,
                            UomId = d.UomId,
                            UnitPrice = d.UnitPrice,
                            DutyMasterId = d.DutyMasterId,
                            FreightAmount = d.FreightAmount,
                            InsuranceAmount = d.InsuranceAmount,
                            CIFValue = d.CIFValue,
                            BasicCustomDuty = d.BasicCustomDuty,
                            SocialWelfareSurCharges = d.SocialWelfareSurCharges,
                            IGST = d.IGST,
                            IGSTPercentage= d.IGSTPercentage,
                            AgriInfraDevCess = d.AgriInfraDevCess,
                            AntiDumpingDuty = d.AntiDumpingDuty,
                            SafeguardDuty = d.SafeguardDuty,
                            HealthEducationCess = d.HealthEducationCess,
                            OtherCharges = d.OtherCharges,
                            TotalValue = d.TotalValue,
                            GRBasedIV = d.GRBasedIV
                        }).ToList()
                    };

                    _db.ImportPOHeader.Add(hEntity);
                }

                foreach (var t in dto.PaymentTerms ?? Enumerable.Empty<ImportPurchasePaymentTermDto>())
                {
                    _db.PurchasePaymentTerms.Add(new PurchasePaymentTerm
                    {
                        PurchaseOrderId = existing.Id,
                        PaymentTermId = t.PaymentTermId,
                        AdvancePercent = t.AdvancePercent,
                        CreditDays = t.CreditDays,
                        PaymentModelId = t.PaymentModelId,
                        InsuranceId = t.InsuranceId,
                        InsurancePercent = t.InsurancePercent,
                        InsuranceAmount = t.InsuranceAmount,
                        AdvanceAmount = t.AdvanceAmount,
                        BalancePercent = t.BalancePercent,
                        BalanceAmount = t.BalanceAmount
                    });
                }

                if (incomingDocs.Count > 0)
                    _db.PurchaseDocuments.AddRange(incomingDocs); 
                    
                await _db.SaveChangesAsync(ct);

                if (impacted.Count > 0)
                    await RecomputeImportIndentPoQtyAsync(impacted, ct);

                await tx.CommitAsync(ct);
                return existing.Id;
            });
        }

        private static void ApplyScalarUpdates(PurchaseOrderHeader existing, PurchaseOrderHeader incoming)
        {

            existing.PODate = incoming.PODate;
            existing.POCategoryId = incoming.POCategoryId;
            existing.POMethodId = incoming.POMethodId;
            existing.CurrencyId = incoming.CurrencyId;
            existing.VendorId = incoming.VendorId;

            existing.ItemTotal = incoming.ItemTotal;
            existing.DiscountTotal = incoming.DiscountTotal;
            existing.PandFTotal = incoming.PandFTotal;
            existing.MiscCharges = incoming.MiscCharges;

            existing.GSTTotal = incoming.GSTTotal;            
            existing.CGSTTotal = incoming.CGSTTotal;
            existing.SGSTTotal = incoming.SGSTTotal;
            existing.IGSTTotal = incoming.IGSTTotal;

            existing.FreightTotal = incoming.FreightTotal;
            existing.InsuranceTotal = incoming.InsuranceTotal;
            existing.TDSTotal = incoming.TDSTotal;
            existing.AdvanceAmount = incoming.AdvanceAmount;
            existing.PurchaseValue = incoming.PurchaseValue;

            existing.BudgetGroupId = incoming.BudgetGroupId;      
            existing.BudgetRequestById = incoming.BudgetRequestById;
            existing.BudgetMonthId = incoming.BudgetMonthId;
            existing.FinancialYearId = incoming.FinancialYearId;

            existing.ProjectId = incoming.ProjectId;
            existing.WBSId = incoming.WBSId;
            existing.BudgetDepartmentId = incoming.BudgetDepartmentId;
            


            existing.RevisionNo = incoming.RevisionNo;
            existing.AmendmentReason = incoming.AmendmentReason;
            existing.StatusId = incoming.StatusId;
        }


        
public async Task<int> AmendAsync(
    PurchaseOrderHeader existing,
    ImportPOUpdateDto dto,
    PurchaseOrderHeader revisedAuditSeed,
    CancellationToken ct)
{
    // status -> Pending
    var pending = await _misc
        .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

    // Build next unique PO number
    var baseRoot = StripRevision(existing.PONumber);
    var nextPoNumber = string.IsNullOrWhiteSpace(dto.PONumber) ? baseRoot : dto.PONumber;
    var nextRevision = Math.Max(existing.RevisionNo, 0);

    while (await _db.PurchaseOrderHeaders.AsNoTracking()
        .AnyAsync(p => p.PONumber == nextPoNumber, ct))
    {
        nextRevision++;
        nextPoNumber = $"{baseRoot}-R{nextRevision}";
    }

    // Impacted indents (old + new)
    var oldImpacted = await GetImpactedIndentIdsFromDbAsync(existing.Id, ct);
    var newImpacted = new HashSet<int>(
        (dto.Headers ?? Enumerable.Empty<ImportPOHeaderDto>())
            .SelectMany(h => h.Details ?? Enumerable.Empty<ImportPODetailDto>())
            .Select(d => d.IndentId )
            .Where(i => i > 0)
            .Distinct());
    var impacted = new HashSet<int>(oldImpacted);
    impacted.UnionWith(newImpacted);

    // Build REVISED root from DTO + audit seed
    var revised = new PurchaseOrderHeader
    {
        // scalars from DTO
        UnitId        = existing.UnitId, // keep same unit
        PONumber      = nextPoNumber,
        PODate        = dto.PODate,
        POCategoryId  = dto.POCategoryId,
        POMethodId    = existing.POMethodId, // Import path fixed
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
        OldPOId       = revisedAuditSeed.OldPOId,
        RevisionNo    = nextRevision,
        AmendmentReason = dto.AmendmentReason?.Trim(),

        CostCenterId  = dto.CostCenterId,
        ProjectId     = dto.ProjectId,
        CapitalTypeId = dto.CapitalTypeId,
        PurchaseTypeId= dto.PurchaseTypeId,
        BudgetGroupId = dto.BudgetGroupId,
        BudgetRequestById = dto.BudgetRequestById,
        BudgetDepartmentId  = dto.BudgetDepartmentId,
        WBSId       = dto.WBSId,
        BudgetMonthId = dto.BudgetMonthId,
        FinancialYearId = dto.FinancialYearId,
        // audit
        CreatedBy     = revisedAuditSeed.CreatedBy,
        CreatedByName = revisedAuditSeed.CreatedByName,
        CreatedIP     = revisedAuditSeed.CreatedIP,
        CreatedDate   = revisedAuditSeed.CreatedDate,
        IsDeleted     = BaseEntity.IsDelete.NotDeleted
    };

    // Prebuild documents EXACTLY from payload (no dedupe)
    var incomingDocs = (dto.Documents ?? new List<DocumentDto>())
        .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
        .Select(d => new PurchaseDocument
        {
            // Id = 0 by default
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
            // 1) DELETE old import children + payment terms + documents
            var oldHeaders = await _db.ImportPOHeader
                .Where(h => h.PurchaseOrderId == existing.Id)
                .Include(h => h.ImportPODetails)
                .ToListAsync(ct);

            if (oldHeaders.Count > 0)
            {
                var oldDetails = oldHeaders.SelectMany(h => h.ImportPODetails).ToList();
                if (oldDetails.Count > 0) _db.ImportPODetail.RemoveRange(oldDetails);
                _db.ImportPOHeader.RemoveRange(oldHeaders);
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

            // 2) SOFT-CLOSE existing header
            var current = await _db.PurchaseOrderHeaders
                .FirstOrDefaultAsync(h => h.Id == existing.Id, ct)
                ?? throw new InvalidOperationException($"PO {existing.Id} not found.");
            current.IsDeleted   = BaseEntity.IsDelete.Deleted;
            current.ModifiedDate= DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            // 3) INSERT revised header
            _db.PurchaseOrderHeaders.Add(revised);
            await _db.SaveChangesAsync(ct); // revised.Id available

            // 4) INSERT revised import children (headers → details) from DTO
            foreach (var h in dto.Headers ?? Enumerable.Empty<ImportPOHeaderDto>())
            {
                var hEntity = new ImportPOHeader
                {
                    PurchaseOrderId     = revised.Id,
                    TTExchangeRateId    = h.TTExchangeRateId,
                    TTExchangeRate    = h.TTExchangeRate,
                    IncotermId          = h.IncotermId,
                    ShippingPortId      = h.ShippingPortId,
                    DestinationPortId   = h.DestinationPortId,
                    ModeOfTransportId   = h.ModeOfTransportId,
                    ShippingModeId      = h.ShippingModeId,
                    CustomsOfficeId     = h.CustomsOfficeId,
                    OriginCountryId     = h.OriginCountryId,
                    InsuranceProviderId = h.InsuranceProviderId,
                    FreightForwarderId  = h.FreightForwarderId,
                    FreeDaysAllowed     = h.FreeDaysAllowed,
                    DemurrageTerms      = h.DemurrageTerms,
                    BillOfLadingNumber  = h.BillOfLadingNumber,
                    VesselName          = h.VesselName,
                    ContainerNumber     = h.ContainerNumber,
                    AirlineName         = h.AirlineName,
                    AirWaybillNumber    = h.AirWaybillNumber,
                    AirWaybillDate      = h.AirWaybillDate,
                    FlightNumber        = h.FlightNumber,
                    ExpectedTimeOfDeparture = h.ExpectedTimeOfDeparture,
                    ExpectedTimeOfArrival   = h.ExpectedTimeOfArrival,
                    CustomsHouseAgentId = h.CustomsHouseAgentId,
                    BillOfEntryNumber   = h.BillOfEntryNumber,
                    LCPaymentModeId     = h.LCPaymentModeId,
                    LCPaymentStatusId   = h.LCPaymentStatusId,
                    LCNumber            = h.LCNumber,
                    LCCurrencyId        = h.LCCurrencyId,
                    LCDate              = h.LCDate,
                    LCExpiryDate        = h.LCExpiryDate,
                    LCAmount            = h.LCAmount,
                    LCIssueBankId       = h.LCIssueBankId,
                    LCBeneficiaryBankId = h.LCBeneficiaryBankId,
                    LCTypeId            = h.LCTypeId,
                    LCRemarks           = h.LCRemarks,
                    TTReferenceNumber   = h.TTReferenceNumber,
                    TTTransferDate      = h.TTTransferDate,
                    TTBankId            = h.TTBankId,
                    TTCurrencyId        = h.TTCurrencyId,
                    TTPaymentModeId     = h.TTPaymentModeId,
                    TTPaymentStatusId   = h.TTPaymentStatusId,
                    TTRemarks           = h.TTRemarks,
                    LCSwiftCode         = h.LCSwiftCode,
                    TTSwiftCode         = h.TTSwiftCode,
                    IsPartialReceiptAllowed = h.IsPartialReceiptAllowed,
                    ImportPODetails     = (h.Details ?? new List<ImportPODetailDto>()).Select(d => new ImportPODetail
                    {
                        IndentId              = d.IndentId,
                        ItemId                = d.ItemId,
                        ItemSno              = d.ItemSno,
                        Quantity              = d.Quantity,
                        UomId                 = d.UomId,
                        UnitPrice             = d.UnitPrice,
                        DutyMasterId          = d.DutyMasterId,
                        FreightAmount         = d.FreightAmount,
                        InsuranceAmount       = d.InsuranceAmount,
                        CIFValue              = d.CIFValue,
                        BasicCustomDuty       = d.BasicCustomDuty,
                        SocialWelfareSurCharges = d.SocialWelfareSurCharges,
                        IGST                  = d.IGST,
                        IGSTPercentage        = d.IGSTPercentage,
                        AgriInfraDevCess      = d.AgriInfraDevCess,
                        AntiDumpingDuty       = d.AntiDumpingDuty,
                        SafeguardDuty         = d.SafeguardDuty,
                        HealthEducationCess   = d.HealthEducationCess,
                        OtherCharges          = d.OtherCharges,
                        TotalValue            = d.TotalValue,
                        GRBasedIV             = d.GRBasedIV
                    }).ToList()
                };
                _db.ImportPOHeader.Add(hEntity);
            }

            foreach (var t in dto.PaymentTerms ?? Enumerable.Empty<ImportPurchasePaymentTermDto>())
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

            // 5) INSERT documents EXACTLY as payload (delete was already done)
            if (incomingDocs.Count > 0)
            {
                foreach (var d in incomingDocs)
                {
                    d.Id   = 0;
                    d.PoId = revised.Id;
                    if (d.UploadedDate == default) d.UploadedDate = DateTimeOffset.UtcNow;
                }
                _db.PurchaseDocuments.AddRange(incomingDocs);
            }

            await _db.SaveChangesAsync(ct);

            // 6) Recompute impacted
            if (impacted.Count > 0)
                await RecomputeImportIndentPoQtyAsync(impacted, ct);

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



        /// <summary>
        /// Same logic as AmendAsync but without managing its own transaction.
        /// The caller must already have an active transaction on _db.Database.
        /// </summary>
        public async Task<int> AmendWithoutTransactionAsync(
            PurchaseOrderHeader existing,
            ImportPOUpdateDto dto,
            PurchaseOrderHeader revisedAuditSeed,
            CancellationToken ct)
        {
            // status -> Pending
            var pending = await _misc
                .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

            // Build next unique PO number
            var baseRoot = StripRevision(existing.PONumber);
            var nextPoNumber = string.IsNullOrWhiteSpace(dto.PONumber) ? baseRoot : dto.PONumber;
            var nextRevision = Math.Max(existing.RevisionNo, 0);

            while (await _db.PurchaseOrderHeaders.AsNoTracking()
                .AnyAsync(p => p.PONumber == nextPoNumber, ct))
            {
                nextRevision++;
                nextPoNumber = $"{baseRoot}-R{nextRevision}";
            }

            // Impacted indents (old + new)
            var oldImpacted = await GetImpactedIndentIdsFromDbAsync(existing.Id, ct);
            var newImpacted = new HashSet<int>(
                (dto.Headers ?? Enumerable.Empty<ImportPOHeaderDto>())
                    .SelectMany(h => h.Details ?? Enumerable.Empty<ImportPODetailDto>())
                    .Select(d => d.IndentId)
                    .Where(i => i > 0)
                    .Distinct());
            var impacted = new HashSet<int>(oldImpacted);
            impacted.UnionWith(newImpacted);

            // Build REVISED root from DTO + audit seed
            var revised = new PurchaseOrderHeader
            {
                UnitId        = existing.UnitId,
                PONumber      = nextPoNumber,
                PODate        = dto.PODate,
                POCategoryId  = dto.POCategoryId,
                POMethodId    = existing.POMethodId,
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
                OldPOId       = revisedAuditSeed.OldPOId,
                RevisionNo    = nextRevision,
                AmendmentReason = dto.AmendmentReason?.Trim(),
                CostCenterId  = dto.CostCenterId,
                ProjectId     = dto.ProjectId,
                CapitalTypeId = dto.CapitalTypeId,
                PurchaseTypeId= dto.PurchaseTypeId,
                BudgetGroupId = dto.BudgetGroupId,
                BudgetRequestById = dto.BudgetRequestById,
                BudgetDepartmentId  = dto.BudgetDepartmentId,
                WBSId       = dto.WBSId,
                BudgetMonthId = dto.BudgetMonthId,
                FinancialYearId = dto.FinancialYearId,
                CreatedBy     = revisedAuditSeed.CreatedBy,
                CreatedByName = revisedAuditSeed.CreatedByName,
                CreatedIP     = revisedAuditSeed.CreatedIP,
                CreatedDate   = revisedAuditSeed.CreatedDate,
                IsDeleted     = BaseEntity.IsDelete.NotDeleted
            };

            // Prebuild documents EXACTLY from payload
            var incomingDocs = (dto.Documents ?? new List<DocumentDto>())
                .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
                .Select(d => new PurchaseDocument
                {
                    DocumentId   = d.DocumentId,
                    FileName     = d.FileName!,
                    UploadedDate = d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
                })
                .ToList();

            // 1) DELETE old import children + payment terms + documents
            var oldHeaders = await _db.ImportPOHeader
                .Where(h => h.PurchaseOrderId == existing.Id)
                .Include(h => h.ImportPODetails)
                .ToListAsync(ct);

            if (oldHeaders.Count > 0)
            {
                var oldDetails = oldHeaders.SelectMany(h => h.ImportPODetails).ToList();
                if (oldDetails.Count > 0) _db.ImportPODetail.RemoveRange(oldDetails);
                _db.ImportPOHeader.RemoveRange(oldHeaders);
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

            // 2) SOFT-CLOSE existing header
            var current = await _db.PurchaseOrderHeaders
                .FirstOrDefaultAsync(h => h.Id == existing.Id, ct)
                ?? throw new InvalidOperationException($"PO {existing.Id} not found.");
            current.IsDeleted   = BaseEntity.IsDelete.Deleted;
            current.ModifiedDate= DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            // 3) INSERT revised header
            _db.PurchaseOrderHeaders.Add(revised);
            await _db.SaveChangesAsync(ct);

            // 4) INSERT revised import children (headers → details) from DTO
            foreach (var h in dto.Headers ?? Enumerable.Empty<ImportPOHeaderDto>())
            {
                var hEntity = new ImportPOHeader
                {
                    PurchaseOrderId     = revised.Id,
                    TTExchangeRateId    = h.TTExchangeRateId,
                    TTExchangeRate    = h.TTExchangeRate,
                    IncotermId          = h.IncotermId,
                    ShippingPortId      = h.ShippingPortId,
                    DestinationPortId   = h.DestinationPortId,
                    ModeOfTransportId   = h.ModeOfTransportId,
                    ShippingModeId      = h.ShippingModeId,
                    CustomsOfficeId     = h.CustomsOfficeId,
                    OriginCountryId     = h.OriginCountryId,
                    InsuranceProviderId = h.InsuranceProviderId,
                    FreightForwarderId  = h.FreightForwarderId,
                    FreeDaysAllowed     = h.FreeDaysAllowed,
                    DemurrageTerms      = h.DemurrageTerms,
                    BillOfLadingNumber  = h.BillOfLadingNumber,
                    VesselName          = h.VesselName,
                    ContainerNumber     = h.ContainerNumber,
                    AirlineName         = h.AirlineName,
                    AirWaybillNumber    = h.AirWaybillNumber,
                    AirWaybillDate      = h.AirWaybillDate,
                    FlightNumber        = h.FlightNumber,
                    ExpectedTimeOfDeparture = h.ExpectedTimeOfDeparture,
                    ExpectedTimeOfArrival   = h.ExpectedTimeOfArrival,
                    CustomsHouseAgentId = h.CustomsHouseAgentId,
                    BillOfEntryNumber   = h.BillOfEntryNumber,
                    LCPaymentModeId     = h.LCPaymentModeId,
                    LCPaymentStatusId   = h.LCPaymentStatusId,
                    LCNumber            = h.LCNumber,
                    LCCurrencyId        = h.LCCurrencyId,
                    LCDate              = h.LCDate,
                    LCExpiryDate        = h.LCExpiryDate,
                    LCAmount            = h.LCAmount,
                    LCIssueBankId       = h.LCIssueBankId,
                    LCBeneficiaryBankId = h.LCBeneficiaryBankId,
                    LCTypeId            = h.LCTypeId,
                    LCRemarks           = h.LCRemarks,
                    TTReferenceNumber   = h.TTReferenceNumber,
                    TTTransferDate      = h.TTTransferDate,
                    TTBankId            = h.TTBankId,
                    TTCurrencyId        = h.TTCurrencyId,
                    TTPaymentModeId     = h.TTPaymentModeId,
                    TTPaymentStatusId   = h.TTPaymentStatusId,
                    TTRemarks           = h.TTRemarks,
                    LCSwiftCode         = h.LCSwiftCode,
                    TTSwiftCode         = h.TTSwiftCode,
                    IsPartialReceiptAllowed = h.IsPartialReceiptAllowed,
                    ImportPODetails     = (h.Details ?? new List<ImportPODetailDto>()).Select(d => new ImportPODetail
                    {
                        IndentId              = d.IndentId,
                        ItemId                = d.ItemId,
                        ItemSno              = d.ItemSno,
                        Quantity              = d.Quantity,
                        UomId                 = d.UomId,
                        UnitPrice             = d.UnitPrice,
                        DutyMasterId          = d.DutyMasterId,
                        FreightAmount         = d.FreightAmount,
                        InsuranceAmount       = d.InsuranceAmount,
                        CIFValue              = d.CIFValue,
                        BasicCustomDuty       = d.BasicCustomDuty,
                        SocialWelfareSurCharges = d.SocialWelfareSurCharges,
                        IGST                  = d.IGST,
                        IGSTPercentage        = d.IGSTPercentage,
                        AgriInfraDevCess      = d.AgriInfraDevCess,
                        AntiDumpingDuty       = d.AntiDumpingDuty,
                        SafeguardDuty         = d.SafeguardDuty,
                        HealthEducationCess   = d.HealthEducationCess,
                        OtherCharges          = d.OtherCharges,
                        TotalValue            = d.TotalValue,
                        GRBasedIV             = d.GRBasedIV
                    }).ToList()
                };
                _db.ImportPOHeader.Add(hEntity);
            }

            foreach (var t in dto.PaymentTerms ?? Enumerable.Empty<ImportPurchasePaymentTermDto>())
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

            // 5) INSERT documents EXACTLY as payload
            if (incomingDocs.Count > 0)
            {
                foreach (var d in incomingDocs)
                {
                    d.Id   = 0;
                    d.PoId = revised.Id;
                    if (d.UploadedDate == default) d.UploadedDate = DateTimeOffset.UtcNow;
                }
                _db.PurchaseDocuments.AddRange(incomingDocs);
            }

            await _db.SaveChangesAsync(ct);

            // 6) Recompute impacted
            if (impacted.Count > 0)
                await RecomputeImportIndentPoQtyAsync(impacted, ct);

            return revised.Id;
        }

        /* ========================= Shared Transaction support ========================= */

        public IExecutionStrategy CreateExecutionStrategy() =>
            _db.Database.CreateExecutionStrategy();

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) =>
            _db.Database.BeginTransactionAsync(ct);

        public async Task<(IDbContextTransaction EfTx, System.Data.Common.DbConnection Conn, System.Data.Common.DbTransaction DbTx)> BeginTransactionWithConnectionAsync(CancellationToken ct)
        {
            var efTx = await _db.Database.BeginTransactionAsync(ct);
            var dbTx = efTx.GetDbTransaction();
            var conn = dbTx.Connection!;
            return (efTx, conn, dbTx);
        }

        /// <summary>
        /// Same logic as CreateAsync but without opening its own transaction.
        /// The caller must already have an active transaction on _db.Database.
        /// </summary>
        public async Task<int> CreateWithoutTransactionAsync(
            PurchaseOrderHeader aggregate, ImportPOCreateDto dto, CancellationToken ct)
        {
            foreach (var h in aggregate.ImportPOHeader ?? Enumerable.Empty<ImportPOHeader>())
            {
                h.Id = 0;
                foreach (var d in h.ImportPODetails ?? Enumerable.Empty<ImportPODetail>())
                    d.Id = 0;
            }
            foreach (var t in aggregate.PaymentTerms ?? Enumerable.Empty<PurchasePaymentTerm>())
                t.Id = 0;
            foreach (var doc in aggregate.PurchaseDocumentTypes ?? Enumerable.Empty<PurchaseDocument>())
                doc.Id = 0;

            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            aggregate.StatusId = pending.Id;

            var impacted = aggregate.ImportPOHeader?
                .SelectMany(h => h.ImportPODetails ?? [])
                .Select(d => d.IndentId ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToHashSet() ?? new HashSet<int>();

            _db.PurchaseOrderHeaders.Add(aggregate);
            await _db.SaveChangesAsync(ct);

            if ((aggregate.PurchaseDocumentTypes == null || aggregate.PurchaseDocumentTypes.Count == 0)
                && dto.Documents != null && dto.Documents.Count > 0)
            {
                var docs = dto.Documents
                    .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
                    .Select(d => new PurchaseDocument
                    {
                        PoId         = aggregate.Id,
                        DocumentId   = d.DocumentId,
                        FileName     = d.FileName,
                        UploadedDate = d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
                    })
                    .ToList();

                if (docs.Count > 0)
                {
                    _db.PurchaseDocuments.AddRange(docs);
                    await _db.SaveChangesAsync(ct);
                }
            }
            else
            {
                if (aggregate.PurchaseDocumentTypes != null && aggregate.PurchaseDocumentTypes.Count > 0)
                {
                    foreach (var doc in aggregate.PurchaseDocumentTypes)
                        doc.PoId = aggregate.Id;
                    await _db.SaveChangesAsync(ct);
                }
            }

            if (impacted.Count > 0)
                await RecomputeImportIndentPoQtyAsync(impacted, ct);

            return aggregate.Id;
        }

        /// <summary>
        /// Same logic as UpdateAsync but without opening its own transaction.
        /// The caller must already have an active transaction on _db.Database.
        /// </summary>
        public async Task<int> UpdateWithoutTransactionAsync(
            PurchaseOrderHeader incoming, ImportPOUpdateDto dto, CancellationToken ct)
        {
            if (dto == null || dto.Id <= 0) return 0;

            var existing = await _db.PurchaseOrderHeaders
                .Include(x => x.ImportPOHeader).ThenInclude(h => h.ImportPODetails)
                .Include(x => x.PaymentTerms)
                .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

            if (existing is null) return 0;

            incoming.StatusId = existing.StatusId;

            var oldIndents = (existing.ImportPOHeader ?? new List<ImportPOHeader>())
                .SelectMany(h => h.ImportPODetails ?? new List<ImportPODetail>())
                .Select(d => d.IndentId.GetValueOrDefault())
                .Where(i => i > 0).Distinct().ToHashSet();

            var newIndents = (dto.Headers ?? new List<ImportPOHeaderDto>())
                .SelectMany(h => h.Details ?? new List<ImportPODetailDto>())
                .Select(d => d.IndentId)
                .Where(i => i > 0).Distinct().ToHashSet();

            var impacted = new HashSet<int>(oldIndents);
            impacted.UnionWith(newIndents);

            var incomingDocs = (dto.Documents ?? new List<DocumentDto>())
                .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
                .GroupBy(d => d.DocumentId)
                .Select(g => g.First())
                .Select(d => new PurchaseDocument
                {
                    PoId         = existing.Id,
                    DocumentId   = d.DocumentId,
                    FileName     = d.FileName!,
                    UploadedDate = d.UploadedDate == default ? DateTimeOffset.UtcNow : d.UploadedDate
                })
                .ToList();

            ApplyScalarUpdates(existing, incoming);
            existing.ModifiedDate = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            if (existing.ImportPOHeader?.Count > 0)
            {
                var oldDetails = existing.ImportPOHeader
                    .SelectMany(h => h.ImportPODetails ?? new List<ImportPODetail>())
                    .ToList();

                if (oldDetails.Count > 0)
                    _db.ImportPODetail.RemoveRange(oldDetails);

                _db.ImportPOHeader.RemoveRange(existing.ImportPOHeader);
            }

            if (existing.PaymentTerms?.Count > 0)
                _db.PurchasePaymentTerms.RemoveRange(existing.PaymentTerms);

            var oldDocs = await _db.PurchaseDocuments
                .Where(d => d.PoId == existing.Id)
                .ToListAsync(ct);

            if (oldDocs.Count > 0)
                _db.PurchaseDocuments.RemoveRange(oldDocs);

            await _db.SaveChangesAsync(ct);

            foreach (var h in dto.Headers ?? Enumerable.Empty<ImportPOHeaderDto>())
            {
                var hEntity = new ImportPOHeader
                {
                    PurchaseOrderId     = existing.Id,
                    TTExchangeRateId    = h.TTExchangeRateId,
                    TTExchangeRate      = h.TTExchangeRate,
                    IncotermId          = h.IncotermId,
                    ShippingPortId      = h.ShippingPortId,
                    DestinationPortId   = h.DestinationPortId,
                    ModeOfTransportId   = h.ModeOfTransportId,
                    ShippingModeId      = h.ShippingModeId,
                    CustomsOfficeId     = h.CustomsOfficeId,
                    OriginCountryId     = h.OriginCountryId,
                    InsuranceProviderId = h.InsuranceProviderId,
                    FreightForwarderId  = h.FreightForwarderId,
                    FreeDaysAllowed     = h.FreeDaysAllowed,
                    DemurrageTerms      = h.DemurrageTerms,
                    BillOfLadingNumber  = h.BillOfLadingNumber,
                    VesselName          = h.VesselName,
                    ContainerNumber     = h.ContainerNumber,
                    AirlineName         = h.AirlineName,
                    AirWaybillNumber    = h.AirWaybillNumber,
                    AirWaybillDate      = h.AirWaybillDate,
                    FlightNumber        = h.FlightNumber,
                    ExpectedTimeOfDeparture = h.ExpectedTimeOfDeparture,
                    ExpectedTimeOfArrival   = h.ExpectedTimeOfArrival,
                    CustomsHouseAgentId = h.CustomsHouseAgentId,
                    BillOfEntryNumber   = h.BillOfEntryNumber,
                    LCPaymentModeId     = h.LCPaymentModeId,
                    LCPaymentStatusId   = h.LCPaymentStatusId,
                    LCNumber            = h.LCNumber,
                    LCDate              = h.LCDate,
                    LCExpiryDate        = h.LCExpiryDate,
                    LCCurrencyId        = h.LCCurrencyId,
                    LCAmount            = h.LCAmount,
                    LCIssueBankId       = h.LCIssueBankId,
                    LCBeneficiaryBankId = h.LCBeneficiaryBankId,
                    LCTypeId            = h.LCTypeId,
                    LCRemarks           = h.LCRemarks,
                    TTReferenceNumber   = h.TTReferenceNumber,
                    TTTransferDate      = h.TTTransferDate,
                    TTBankId            = h.TTBankId,
                    TTCurrencyId        = h.TTCurrencyId,
                    TTPaymentModeId     = h.TTPaymentModeId,
                    TTPaymentStatusId   = h.TTPaymentStatusId,
                    TTRemarks           = h.TTRemarks,
                    LCSwiftCode         = h.LCSwiftCode,
                    TTSwiftCode         = h.TTSwiftCode,
                    IsPartialReceiptAllowed = h.IsPartialReceiptAllowed,
                    ImportPODetails = (h.Details ?? new List<ImportPODetailDto>()).Select(d => new ImportPODetail
                    {
                        IndentId                = d.IndentId,
                        ItemId                  = d.ItemId,
                        ItemSno                 = d.ItemSno,
                        Quantity                = d.Quantity,
                        UomId                   = d.UomId,
                        UnitPrice               = d.UnitPrice,
                        DutyMasterId            = d.DutyMasterId,
                        FreightAmount           = d.FreightAmount,
                        InsuranceAmount         = d.InsuranceAmount,
                        CIFValue                = d.CIFValue,
                        BasicCustomDuty         = d.BasicCustomDuty,
                        SocialWelfareSurCharges = d.SocialWelfareSurCharges,
                        IGST                    = d.IGST,
                        IGSTPercentage          = d.IGSTPercentage,
                        AgriInfraDevCess        = d.AgriInfraDevCess,
                        AntiDumpingDuty         = d.AntiDumpingDuty,
                        SafeguardDuty           = d.SafeguardDuty,
                        HealthEducationCess     = d.HealthEducationCess,
                        OtherCharges            = d.OtherCharges,
                        TotalValue              = d.TotalValue,
                        GRBasedIV               = d.GRBasedIV
                    }).ToList()
                };

                _db.ImportPOHeader.Add(hEntity);
            }

            foreach (var t in dto.PaymentTerms ?? Enumerable.Empty<ImportPurchasePaymentTermDto>())
            {
                _db.PurchasePaymentTerms.Add(new PurchasePaymentTerm
                {
                    PurchaseOrderId  = existing.Id,
                    PaymentTermId    = t.PaymentTermId,
                    AdvancePercent   = t.AdvancePercent,
                    CreditDays       = t.CreditDays,
                    PaymentModelId   = t.PaymentModelId,
                    InsuranceId      = t.InsuranceId,
                    InsurancePercent = t.InsurancePercent,
                    InsuranceAmount  = t.InsuranceAmount,
                    AdvanceAmount    = t.AdvanceAmount,
                    BalancePercent   = t.BalancePercent,
                    BalanceAmount    = t.BalanceAmount
                });
            }

            if (incomingDocs.Count > 0)
                _db.PurchaseDocuments.AddRange(incomingDocs);

            await _db.SaveChangesAsync(ct);

            if (impacted.Count > 0)
                await RecomputeImportIndentPoQtyAsync(impacted, ct);

            return existing.Id;
        }

        /* ========================= Helpers ========================= */

        private static void MarkGraphAsAdded(PurchaseOrderHeader root)
        {
            // mark root
            // The context will re-evaluate once attached; this is a pre-attach intent.
            root.Id = 0;

            if (root.ImportPOHeader != null)
            {
                foreach (var h in root.ImportPOHeader)
                {
                    h.Id = 0;
                    if (h.ImportPODetails != null)
                        foreach (var d in h.ImportPODetails) d.Id = 0;
                }
            }
            if (root.PaymentTerms != null)
                foreach (var t in root.PaymentTerms) t.Id = 0;
        }

        private static HashSet<int> GetImpactedIndentIdsFromPo(PurchaseOrderHeader po)
        {
            return (po.ImportPOHeader ?? Enumerable.Empty<ImportPOHeader>())
                .SelectMany(h => h.ImportPODetails ?? Enumerable.Empty<ImportPODetail>())
                .Select(d => d.IndentId ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToHashSet();
        }

        private async Task<HashSet<int>> GetImpactedIndentIdsFromDbAsync(int poId, CancellationToken ct)
        {
            var ids = await (
                from lh in _db.ImportPOHeader.AsNoTracking()
                where lh.PurchaseOrderId == poId
                join d in _db.ImportPODetail.AsNoTracking() on lh.Id equals d.PurchaseHeaderId
                where d.IndentId != null && d.IndentId > 0
                select d.IndentId!.Value
            ).Distinct().ToListAsync(ct);

            return ids.ToHashSet();
        }

        private async Task RecomputeImportIndentPoQtyAsync(HashSet<int> indentHeaderIds, CancellationToken ct)
        {
            if (indentHeaderIds.Count == 0) return;

            var sums = await _db.ImportPODetail
                .Where(d => d.IndentId != null && indentHeaderIds.Contains(d.IndentId.Value))
                .Join(_db.ImportPOHeader, d => d.PurchaseHeaderId, lh => lh.Id, (d, lh) => new { d, lh })
                .Where(x => x.lh.IsDeleted == 0)
                .Join(_db.PurchaseOrderHeaders, x => x.lh.PurchaseOrderId, h => h.Id, (x, h) => new { x.d, h })
                .Where(y => y.h.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                .GroupBy(y => new { IndentHeaderId = y.d.IndentId!.Value, y.d.ItemId })
                .Select(g => new { g.Key.IndentHeaderId, g.Key.ItemId, Qty = g.Sum(z => z.d.Quantity) })
                .ToListAsync(ct);

            var qtyMap = sums.ToDictionary(k => (k.IndentHeaderId, k.ItemId), v => v.Qty);

            var indentRows = await _db.IndentDetail
                .Where(r => r.IsDeleted == 0 && indentHeaderIds.Contains(r.IndentHeaderId))
                .ToListAsync(ct);

            foreach (var row in indentRows)
            {
                qtyMap.TryGetValue((row.IndentHeaderId, row.ItemId), out var totalQty);
                row.POQty = totalQty;
            }

            await _db.SaveChangesAsync(ct);
        }

        private static string StripRevision(string code)
        {
            var idx = code.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0 && idx + 2 < code.Length && int.TryParse(code[(idx + 2)..], out _))
                return code[..idx];
            return code;
        }

        public Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct) => _db.PurchaseOrderHeaders.Include(h => h.ImportPOHeader).ThenInclude(l => l.ImportPODetails).Include(h => h.PaymentTerms).FirstOrDefaultAsync(h => h.Id == id, ct);

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

        /* ========================= CANCEL ========================= */
        public async Task<bool> CancelAsync(int id, CancellationToken ct)
        {
            var existing = await _db.PurchaseOrderHeaders
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var cancelledStatus = await _misc.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Cancelled);
            existing.StatusId = cancelledStatus?.Id ?? existing.StatusId;

            existing.CancelledDate = DateTimeOffset.UtcNow;
            existing.CancelledByName = _ipAddressService.GetUserName();
            existing.CancelledIP = _ipAddressService.GetUserIPAddress();

            _db.PurchaseOrderHeaders.Update(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        /* ========================= FORECLOSE ========================= */
        public async Task<bool> ForecloseAsync(int id, CancellationToken ct)
        {
            var existing = await _db.PurchaseOrderHeaders
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var foreclosedStatus = await _misc.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus, MiscEnumEntity.ForeClosed);
            existing.StatusId = foreclosedStatus?.Id ?? existing.StatusId;

            existing.ForeClosedDate = DateTimeOffset.UtcNow;
            existing.ForeClosedByName = _ipAddressService.GetUserName();
            existing.ForeClosedIP = _ipAddressService.GetUserIPAddress();

            _db.PurchaseOrderHeaders.Update(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

