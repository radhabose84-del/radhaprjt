using System.Data;
using Contracts.Dtos.Inventory;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IParty;
using Contracts.Interfaces.External.IUser;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationEntry;

public class QuotationQueryRepository(
    ApplicationDbContext db,
    IItemGrpcClient itemGrpc,
    IUOMGrpcClient uomGrpc,
    IPartyGrpcClient partyGrpc  ,IIPAddressService ip    ,ICurrencyGrpcClient currencyGrpc,ICompanyGrpcClient companyGrpcClient,IUnitGrpcClient unitGrpcClient
) : IQuotationQueryRepository
{
    public async Task<(List<QuotationListItemDto> Items, int Total)> GetAllAsync(
    int PageNumber,
    int PageSize,
    string? SearchTerm)
    {
        var unitId = ip.GetUnitId();

        var q = db.Set<QuotationHeader>()
                .AsNoTracking()
                .Where(h => h.IsDeleted == BaseEntity.IsDelete.NotDeleted);

/*         if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim();
            q = q.Where(h => h.QuotationNumber.Contains(term));
        } */

        var total = await q.CountAsync();

        var compHeaders = db.Set<QuotationComparisonHeader>();

        var page = await q.OrderByDescending(h => h.Id)
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize)
                        .Select(h => new
                        {
                            h.Id,
                            h.QuotationNumber,
                            h.SupplierId,
                            h.RfqId,
                            h.ValidTill,
                            h.FreightModeId,
                            h.Freight,
                            h.PaymentTermsId,
                            h.IncotermsId,
                            h.InsuranceCharge,
                            h.TaxableSubtotal,
                            h.GstTotal,
                            h.ItemsTotal,
                            h.GrandTotal,
                            h.IsActive,
                            h.QuotationImage,
                            HasComparison = compHeaders.Any(ch => ch.RfqId == h.RfqId)
                        })
                        .ToListAsync();

        if (page.Count == 0)
            return (new List<QuotationListItemDto>(), total);

        var supplierIds = page.Select(p => p.SupplierId).Distinct().ToList();
        var rfqIds      = page.Select(p => p.RfqId).Distinct().ToList();
        var miscIds     = page.SelectMany(p => new[] { p.FreightModeId, p.PaymentTermsId, p.IncotermsId })
                            .Where(id => id != 0)
                            .Distinct()
                            .ToList();

        var rfqMap = await db.Set<RfqMaster>()
                            .AsNoTracking()
                            .Where(r => rfqIds.Contains(r.Id))
                            .Where(r => r.UnitId == unitId)
                            .Select(r => new { r.Id, r.RfqCode })
                            .ToDictionaryAsync(k => k.Id, v => v.RfqCode ?? string.Empty);

        var miscMap = await db.Set<Core.Domain.Entities.MiscMaster>()
                            .AsNoTracking()
                            .Where(m => miscIds.Contains(m.Id))
                            .Select(m => new { m.Id, m.Code })
                            .ToDictionaryAsync(k => k.Id, v => v.Code ?? string.Empty);

        var supplierTasks = supplierIds.Select(id => partyGrpc.GetPartyByIdAsync(id)).ToArray();
        await Task.WhenAll(supplierTasks);
        var supplierMap = supplierTasks.Where(t => t.Result != null)
                                    .Select(t => t.Result!)
                                    .ToDictionary(k => k.Id, v => v.PartyName ?? string.Empty);

        var items = page.Select(p => new QuotationListItemDto(
            Id:               p.Id,
            QuotationNumber:  p.QuotationNumber,
            SupplierName:     supplierMap.TryGetValue(p.SupplierId, out var sName) ? sName : string.Empty,
            RfqNumber:        rfqMap.TryGetValue(p.RfqId, out var rCode) ? rCode : string.Empty,
            ValidTill:        p.ValidTill,
            FreightModeName:  miscMap.TryGetValue(p.FreightModeId ?? 0, out var fm) ? fm : string.Empty,
            Freight:          p.Freight ?? 0,
            PaymentTermsName: miscMap.TryGetValue(p.PaymentTermsId ?? 0, out var pt) ? pt : string.Empty,
            IncotermsName:    miscMap.TryGetValue(p.IncotermsId ?? 0, out var it) ? it : string.Empty,
            InsuranceCharge:  p.InsuranceCharge ?? 0,
            TaxableSubtotal:  p.TaxableSubtotal,
            GstTotal:         p.GstTotal,
            ItemsTotal:       p.ItemsTotal,
            GrandTotal:       p.GrandTotal,
            IsActive:         (p.IsActive == BaseEntity.Status.Active ? 1 : 0),
            QuotationImage:   p.QuotationImage,
            Edit:             p.HasComparison ? 1 : 0,
            EditReason:       p.HasComparison ? "Quotation comparison exists" : null
        )).ToList();

        return (items, total);
    }


    public async Task<GetQuotationHeaderDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        // Read the two config values sequentially (EF)
        var basePath = await db.MiscTypeMaster
            .Where(t => t.MiscTypeCode == MiscEnumEntity.ImagePath && t.IsDeleted == 0)
            .Select(t => t.Description)
            .FirstOrDefaultAsync(ct);

        var folder = await db.MiscTypeMaster
            .Where(t => t.MiscTypeCode == MiscEnumEntity.QuotationImage && t.IsDeleted == 0)
            .Select(t => t.Description)
            .FirstOrDefaultAsync(ct);

         var companies = await companyGrpcClient.GetAllCompanyAsync();
        var units = await unitGrpcClient.GetAllUnitAsync();

        var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
        var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
        var oldUnitLookup = units.Where(u => !string.IsNullOrEmpty(u.OldUnitId))
                            .ToDictionary(u => u.OldUnitId, u => u.UnitName);

        var CompanyName = companyLookup.TryGetValue(ip.GetCompanyId(), out var cName) ? cName : string.Empty;
        var UnitName = unitLookup.TryGetValue(ip.GetUnitId(), out var uName) ? uName : string.Empty;

        string prefix = string.Empty;
        if (!string.IsNullOrWhiteSpace(basePath) && !string.IsNullOrWhiteSpace(folder))
        {
            var b = basePath.TrimEnd('/', '\\');
            var f = folder.Trim('/', '\\');
            prefix = $"{b}/{f}/{CompanyName}/{UnitName}/";
        }

        // 1) Header + Lines (EF)
        var h = await db.Set<QuotationHeader>()
                        .AsNoTracking()
                        .Include(x => x.Lines)
                        .FirstOrDefaultAsync(x =>
                            x.Id == id &&
                            x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

        if (h is null) return null;

        var linesNotDeleted = h.Lines
            .Where(l => l.IsDeleted == BaseEntity.IsDelete.NotDeleted)
            .ToList();

        // 2) Distinct IDs
        var itemIds = linesNotDeleted.Select(l => l.ItemId).Distinct().ToList();
        var uomIds  = linesNotDeleted.Select(l => l.UomId).Where(x => x != 0).Distinct().ToList();
        var currencyIds  = linesNotDeleted.Select(l => l.CurrencyId).Where(x => x != 0).Distinct().ToList();
       

        // 3) Kick off ONLY gRPC calls in parallel
        var itemsTask    = itemGrpc.GetItemsByIdsAsync(itemIds, ct); 
        var supplierTask = partyGrpc.GetPartyByIdAsync(h.SupplierId); 
        var currencyTask = currencyGrpc.GetByIdsAsync(currencyIds,ct); 

        // 4) EF queries MUST be awaited sequentially
        var rfq = await db.Set<RfqMaster>()
                        .AsNoTracking()
                        .Where(r => r.Id == h.RfqId)
                        .Select(r => new { r.Id, r.RfqCode, r.RfqStatusId })
                        .FirstOrDefaultAsync(ct);

        var miscIds = new[] { h.FreightModeId, h.PaymentTermsId, h.IncotermsId }
                    .Where(x => x != 0).Distinct().ToList();

        var miscMap = await db.Set<Core.Domain.Entities.MiscMaster>()
                            .AsNoTracking()
                            .Where(m => miscIds.Contains(m.Id))
                            .Select(m => new { m.Id, m.Code })
                            .ToDictionaryAsync(k => k.Id, v => v.Code ?? string.Empty, ct);

        // 5) Now await gRPC tasks (safe to run in parallel)
        var items    = (await itemsTask).ToDictionary(k => k.Id, v => v);
        var supplier = supplierTask.Result;        
        var currencies = (await currencyTask).ToDictionary(k => k.Id, v => v);
        
        // 6) UOM fan-out (gRPC), fine to parallelize per-id
        var uomMap = new Dictionary<int, UOMMasterDto>();
        if (uomIds.Count > 0)
        {
            var uomTasks = uomIds.Select(id2 => uomGrpc.GetByIdAsync(id2, ct)).ToArray();
            var results  = await Task.WhenAll(uomTasks);
            foreach (var u in results.Where(x => x != null))
                uomMap[u!.Id] = u!;
        }        
        var lines = linesNotDeleted.Select(l =>
        {
            items.TryGetValue(l.ItemId, out var im);            
            currencies.TryGetValue(l.CurrencyId, out var cm);

            var uomName = uomMap.TryGetValue(l.UomId, out var uom)
                            ? (uom.UOMName ?? string.Empty)
                            : string.Empty;           
            return new GetQuotationDetailDto(
                ItemId: l.ItemId,
                ItemCode: im?.ItemCode ?? string.Empty,
                ItemName: im?.ItemName ?? string.Empty,
                HsnId: l.HsnId,                
                UomId: l.UomId,
                CurrencyId: l.CurrencyId,
                CurrencyName: cm?.Code ?? string.Empty,
                UomName: uomName,
                Quantity: l.Quantity,
                Rate: l.Rate,
                DiscountTypeId: l.DiscountTypeId??0,
                Discount: l.Discount??0,
                PandFCharge: l.PandFCharge??0,
                GstPercent: l.GstPercent,
                Warranty: l.Warranty??0,
                ValidityDays: l.ValidityDays??0,
                DeliveryDays: l.DeliveryDays??0,
                LineSubtotal: l.LineSubtotal,
                GstAmount: l.GstAmount,
                Total: l.Total,
                IsActive: (l.IsActive == BaseEntity.Status.Active ? 1 : 0)
            );
        }).ToList();

        // 8) Header projection
        return new GetQuotationHeaderDto(
            Id: h.Id,
            QuotationNumber: h.QuotationNumber,
            SupplierId: h.SupplierId,
            SupplierName: supplier?.PartyName ?? string.Empty,
            RfqId: h.RfqId,
            RfqNumber: rfq?.RfqCode ?? string.Empty,
            ValidTill: h.ValidTill,
            FreightModeId: h.FreightModeId??0,
            FreightModeName: miscMap.TryGetValue(h.FreightModeId??0, out var fName) ? fName : string.Empty,
            Freight: h.Freight??0,
            PaymentTermsId: h.PaymentTermsId??0,
            PaymentTermsName: miscMap.TryGetValue(h.PaymentTermsId??0, out var pName) ? pName : string.Empty,
            IncotermsId: h.IncotermsId??0,
            IncotermsName: miscMap.TryGetValue(h.IncotermsId??0, out var iName) ? iName : string.Empty,
            TaxableSubtotal: h.TaxableSubtotal,
            InsuranceCharge: h.InsuranceCharge??0,
            GstTotal: h.GstTotal,
            ItemsTotal: h.ItemsTotal,
            GrandTotal: h.GrandTotal,
            IsActive: (h.IsActive == BaseEntity.Status.Active ? 1 : 0),
            QuotationImage: h.QuotationImage,
            ImageUrl: !string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(h.QuotationImage)
                        ? prefix + h.QuotationImage
                        : null,
            Lines: lines
        );
    }


    public async Task<List<QuotationAutoCompleteDto>> GetQuotationAutoComplete(string? searchPattern)
    {
        var unitId = ip.GetUnitId();
        var q = db.Set<QuotationHeader>()
                .AsNoTracking()
                .Where(h => h.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                            h.IsActive == BaseEntity.Status.Active);

        if (!string.IsNullOrWhiteSpace(searchPattern))
        {
            var term = searchPattern.Trim();
            q = q.Where(h => h.QuotationNumber.Contains(term));
        }

        // Pull base info
        var baseList = await q.OrderByDescending(h => h.Id)
                            .Take(10)
                            .Select(h => new
                            {
                                h.Id,
                                h.QuotationNumber,
                                h.SupplierId,
                                h.RfqId
                            })
                            .ToListAsync();

        if (baseList.Count == 0) return new List<QuotationAutoCompleteDto>();

        // Collect ids for enrichment
        var supplierIds = baseList.Select(b => b.SupplierId).Distinct().ToList();
        var rfqIds = baseList.Select(b => b.RfqId).Distinct().ToList();

        // Supplier names via gRPC (fan-out; adjust if you add batch later)
        var supplierTasks = supplierIds.Select(id => partyGrpc.GetPartyByIdAsync(id)).ToArray();
        await Task.WhenAll(supplierTasks);
        var supplierMap = supplierTasks
            .Where(t => t.Result != null)
            .Select(t => t.Result!)
            .ToDictionary(k => k.Id, v => v.PartyName ?? string.Empty);

        // RFQ numbers via EF
        var rfqMap = await db.Set<RfqMaster>()
                            .AsNoTracking()
                            .Where(r => rfqIds.Contains(r.Id))
                            .Where(r => r.UnitId == unitId)
                            .Select(r => new { r.Id, r.RfqCode })
                            .ToDictionaryAsync(k => k.Id, v => v.RfqCode ?? string.Empty);

        // Project final DTOs
        return baseList.Select(b => new QuotationAutoCompleteDto(
            Id: b.Id,
            QuotationNumber: b.QuotationNumber,
            SupplierName: supplierMap.TryGetValue(b.SupplierId, out var sName) ? sName : string.Empty,
            SupplierId: b.SupplierId,
            RfqNumber: rfqMap.TryGetValue(b.RfqId, out var rCode) ? rCode : string.Empty,
            RfqId: b.RfqId
        )).ToList();
    }
     
}
