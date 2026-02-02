using System.Data;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos;
using Dapper;
using Contracts.Interfaces.External.IParty;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IUser;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Application.PurchaseOrder.UploadPODocument;
using Contracts.Dtos.Users;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local; // IPartyGrpcClient (adjust if your namespace differs)

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ImportPO;

public class ImportPOQueryRepository : IImportPOQueryRepository
{
    private readonly IDbConnection _conn;
    private readonly IIPAddressService _ip;
    private readonly IPartyGrpcClient _partyGrpc;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
    private readonly IUOMGrpcClient _uomGrpc;
    private readonly IItemGrpcClient _itemGrpcClient;
    private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
    private readonly ICurrencyGrpcClient _currencyGrpc;
    private readonly ICompanyGrpcClient _companyGrpcClient;     
    private readonly IUnitGrpcClient _unitGrpcClient;


    public ImportPOQueryRepository(
    IDbConnection conn,
    IIPAddressService ip,
    IPartyGrpcClient partyGrpc, IMiscMasterQueryRepository miscMasterQueryRepository, IUOMGrpcClient uomGrpc, IItemGrpcClient itemGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient, ICurrencyGrpcClient currencyGrpc,ICompanyGrpcClient companyGrpcClient,IUnitGrpcClient unitGrpcClient)
    {
        _conn = conn;
        _ip = ip;
        _partyGrpc = partyGrpc;
        _miscMasterQueryRepository = miscMasterQueryRepository;
        _uomGrpc = uomGrpc;
        _itemGrpcClient = itemGrpcClient;
        _departmentAllGrpcClient = departmentAllGrpcClient;
        _currencyGrpc = currencyGrpc;
        _companyGrpcClient = companyGrpcClient;  
        _unitGrpcClient=unitGrpcClient; 
    }

    public async Task<ImportPOFullVm?> GetByIdAsync(int id, CancellationToken ct)
    {
        var companies = await _companyGrpcClient.GetAllCompanyAsync();
        var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
        var units = await _unitGrpcClient.GetAllUnitAsync();

        var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
        var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
        var oldUnitLookup = units.Where(u => !string.IsNullOrEmpty(u.OldUnitId))
                            .ToDictionary(u => u.OldUnitId, u => u.UnitName);
        var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

        var CompanyName = companyLookup.TryGetValue(_ip.GetCompanyId(), out var cName) ? cName : string.Empty;
        var UnitName = unitLookup.TryGetValue(_ip.GetUnitId(), out var uName) ? uName : string.Empty;    

        // 1) PO header summary (strictly non-deleted)
        var po = await _conn.QueryFirstOrDefaultAsync<PurchaseOrderHeaderSummaryDto>(
            @"SELECT poh.Id, poh.PONumber, poh.PODate, poh.VendorId, poh.CurrencyId,
                    poh.ItemTotal, poh.DiscountTotal, poh.PandFTotal, poh.MiscCharges,
                    poh.GSTTotal,  poh.IGSTTotal, poh.FreightTotal, poh.InsuranceTotal,
                    poh.AdvanceAmount, poh.PurchaseValue, poh.StatusId,poh.unitId,POCategoryId,POMethodId,BudgetGroupId,AmendmentReason,RevisionNo,CapitalTypeId,CostCenterId,ProjectId,PurchaseTypeId  ,BudgetGroupId,BudgetRequestById,BudgetDepartmentId ,WBSId 
            FROM Purchase.PurchaseOrderHeader poh WITH (NOLOCK)
            WHERE poh.Id = @id AND poh.IsDeleted = 0;",
            new { id });

        if (po is null) return null;

        try
        {
            var vendorIds = new List<int> { po.VendorId }.Where(v => v > 0).Distinct().ToList();
            var tasks = vendorIds.Select(vId => _partyGrpc.GetPartyByIdAsync(vId)).ToArray();
            await Task.WhenAll(tasks);

            var map = tasks
                .Where(t => t.Status == TaskStatus.RanToCompletion && t.Result != null)
                .Select(t => t.Result!)
                .ToDictionary(x => x.Id, x => x.PartyName ?? string.Empty);

            if (map.TryGetValue(po.VendorId, out var vname))
                po.VendorName = vname;                   // ← set it here
        }
        catch
        {
            // swallow vendor enrichment failures
        }
 try
        {
            var currencyIds = new List<int> { po.CurrencyId }
                .Where(c => c > 0)
                .Distinct()
                .ToList();

            // both branches are List<CurrencyDto>
            var list = currencyIds.Count == 0
                ? new List<CurrencyDto>()
                : (await _currencyGrpc.GetByIdsAsync(currencyIds, ct)).ToList();

            var map = list.ToDictionary(c => c.Id, c => c.Code ?? c.Name ?? string.Empty);

            if (map.TryGetValue(po.CurrencyId, out var cname))
                po.CurrencyName = cname;
        }
        catch
        {
            // swallow currency enrichment failures
        }
        // 2) Payment Terms
        var terms = (await _conn.QueryAsync<PurchasePaymentTermReadDto>(
            @"SELECT ppt.Id, ppt.PaymentTermId, ppt.AdvancePercent, ppt.CreditDays,
                    ppt.PaymentModelId, ppt.InsuranceId, ppt.InsurancePercent, ppt.InsuranceAmount,
                    ppt.AdvanceAmount, ppt.BalancePercent, ppt.BalanceAmount
            FROM Purchase.PurchasePaymentTerm ppt WITH (NOLOCK)
            WHERE ppt.PurchaseOrderId = @id AND ppt.IsDeleted = 0
            ORDER BY ppt.Id;",
            new { id })).ToList();

        // 3) Import Headers
        var headers = (await _conn.QueryAsync<ImportPOHeaderReadDto>(
            @"SELECT iph.Id, iph.PurchaseOrderId, iph.TTExchangeRateId , iph.IncotermId, iph.ShippingPortId,
                    iph.DestinationPortId, iph.ModeOfTransportId,iph.ShippingModeId, iph.CustomsOfficeId, iph.OriginCountryId,
                    iph.InsuranceProviderId, iph.FreightForwarderId,iph.FreeDaysAllowed,
                    iph.DemurrageTerms, iph.BillOfLadingNumber, iph.VesselName, iph.ContainerNumber,
                    iph.AirlineName, iph.AirWaybillNumber, iph.AirWaybillDate, iph.FlightNumber,
                    iph.ExpectedTimeOfDeparture, iph.ExpectedTimeOfArrival, iph.CustomsHouseAgentId,
                    iph.BillOfEntryNumber, iph.LCNumber,iph.LcCurrencyId, iph.LCDate, iph.LCExpiryDate, iph.LCAmount,
                    iph.LCIssueBankId, iph.LCBeneficiaryBankId, iph.LCTypeId, iph.TTReferenceNumber,
                    iph.TTTransferDate, iph.TTBankId, iph.TTPaymentStatusId, iph.TTRemarks,LCPaymentModeId,LCPaymentStatusId,LCRemarks,TTCurrencyId,TTPaymentModeId,LCSwiftCode,TTSwiftCode,iph.TTExchangeRate,IsPartialReceiptAllowed
            FROM Purchase.PurchaseOrderImportHeader iph WITH (NOLOCK)
            WHERE iph.PurchaseOrderId = @id AND iph.IsDeleted = 0
            ORDER BY iph.Id;",
            new { id })).ToList();

        // 4) Import Details for ALL headers of this PO (non-deleted)
        var details = (await _conn.QueryAsync<ImportPODetailReadDto>(
            @"SELECT ipd.Id, ipd.PurchaseHeaderId, ipd.IndentId, ipd.ItemId,ipd.ItemSno, ipd.UomId, ipd.UnitPrice,
                    ipd.DutyMasterId, ipd.FreightAmount, ipd.InsuranceAmount, ipd.CIFValue,
                    ipd.BasicCustomDuty, ipd.SocialWelfareSurCharges, ipd.IGST,ipd.IGSTPercentage,
                    ipd.AgriInfraDevCess, ipd.AntiDumpingDuty, ipd.SafeguardDuty, ipd.HealthEducationCess,
                    ipd.OtherCharges, ipd.TotalValue, ipd.GRBasedIV AS GRBasedIV,ipd.Quantity
            FROM Purchase.PurchaseOrderImportDetail ipd WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseOrderImportHeader iph WITH (NOLOCK) ON iph.Id = ipd.PurchaseHeaderId
            WHERE iph.PurchaseOrderId = @id 
            ORDER BY ipd.Id;",
            new { id })).ToList();
            
       var documents = (await _conn.QueryAsync<DocumentDtoList>(
                @"SELECT PurchaseDocuments.Id, POId, DocumentId, FileName,  UploadedDate,
                MiscMaster.Code DocumentName,MiscTypeMaster.Description+'/'+MT.Description+'/'+ @companyName + '/' + @unitName+'/'+filename AS UploadedPath
                FROM Purchase.PurchaseDocuments WITH (NOLOCK)
                inner join purchase.MiscMaster on MiscMaster.Id = DocumentId                 
				left join purchase.MiscTypeMaster on MiscTypeCode='ImagePath'
				left join purchase.MiscTypeMaster MT on MT.MiscTypeCode='POImage'
                WHERE POId = @id
                ORDER BY Id;",
                new { id, companyName = CompanyName,
                    unitName = UnitName })).ToList();
         

        // 5) Compute edit gate like Local
        var hasGrn = await HasAnyGrnAsync(id, ct);
        var statusCode = await GetStatusCodeAsync(id, ct) ?? string.Empty;
        var (editFlag, editReason) = ComputeEditGate(hasGrn, statusCode);

        // If your PurchaseOrderHeaderSummaryDto has these members, set them.
        // Otherwise, add them to the DTO (int Edit, string? EditReason) to keep parity with Local.
        try
        {
            // dynamic assignment guarded in case properties exist
            var poType = po.GetType();
            var editProp = poType.GetProperty("Edit");
            var reasonProp = poType.GetProperty("EditReason");
            editProp?.SetValue(po, editFlag);
            reasonProp?.SetValue(po, editReason);
        }
        catch { /* ignore if properties don't exist */ }

        // 6) Enrichment (UOM, Item, Indent) — mirrors Local flow

        if (headers.Count > 0 && details.Count > 0)
        {
            var itemIds = details.Select(x => x.ItemId).Where(x => x > 0).Distinct().ToList();
            var uomIds = details.Select(x => x.UomId).Where(x => x > 0).Distinct().ToList();
            var indentIds = details.Select(x => x.IndentId).Where(x => x > 0).Distinct().ToArray();

            // Kick off lookups
            var uomsTask = _uomGrpc.GetUOMAsync();                  // List<UOMDto>
            var itemsTask = _itemGrpcClient.GetItemsByIdsAsync(itemIds, ct);

            Task<IEnumerable<dynamic>> indentRowsTask = Task.FromResult<IEnumerable<dynamic>>(Array.Empty<dynamic>());
            if (indentIds.Length > 0)
            {
                indentRowsTask = _conn.QueryAsync(
                    @"SELECT Id, IndentNumber
                    FROM Purchase.IndentHeader WITH (NOLOCK)
                    WHERE IsDeleted = 0 AND Id IN @ids;",
                    new { ids = indentIds });
            }

            await Task.WhenAll(uomsTask, itemsTask, indentRowsTask);

            // Build maps
            var uomMap = new Dictionary<int, string>();
            var uoms = await uomsTask;
            if (uoms.Any())
            {
                uomMap = uoms
                    .Where(u => uomIds.Contains(u.Id))
                    .GroupBy(u => u.Id)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var u = g.First();
                            return !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName! : (u.Code ?? string.Empty);
                        });
            }

            var itemMap = new Dictionary<int, string>();
            var items = await itemsTask;
            if (items.Any())
            {
                itemMap = items
                    .GroupBy(i => i.Id)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var it = g.First();
                            return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName! : (it.ItemCode ?? string.Empty);
                        });
            }

            var indentMap = new Dictionary<int, string>();
            var indentRows = await indentRowsTask;
            foreach (var r in indentRows)
            {
                int rid = (int)r.Id;
                string num = (string)r.IndentNumber;
                indentMap[rid] = num;
            }

            // Apply enrichment to each detail
            foreach (var d in details)
            {
                if (d.UomId > 0 && uomMap.TryGetValue(d.UomId, out var uomName))
                    d.UomName = uomName;                 // ensure your ReadDto has UomName

                if (d.ItemId > 0 && itemMap.TryGetValue(d.ItemId, out var itemName))
                    d.ItemName = itemName;               // ensure your ReadDto has ItemName

                if (d.IndentId > 0 && indentMap.TryGetValue(d.IndentId, out var indentNo))
                    d.IndentNumber = indentNo;           // ensure your ReadDto has IndentNumber
            }
        }

        {
            var detailsLookup = details
                .GroupBy(d => d.PurchaseHeaderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var h in headers)
                if (detailsLookup.TryGetValue(h.Id, out var d)) h.Details = d;
        }

        // 8) Return VM (PO + Terms + Headers)
        return new ImportPOFullVm
        {
            PO = po,
            PaymentTerms = terms,
            ImportHeaders = headers,
            ImportDocumentList= documents
        };
    }
    public async Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
                SELECT TOP 1 1
                FROM Purchase.GRNDetail g
                WHERE g.PoId = @poId;";

        var exists = await _conn.ExecuteScalarAsync<int?>(sql, new { poId });
        return exists.HasValue;
    }

    public async Task<string?> GetStatusCodeAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
                SELECT m.Code
                FROM Purchase.PurchaseOrderHeader h
                LEFT JOIN Purchase.MiscMaster m ON m.Id = h.StatusId
                WHERE h.Id = @poId AND h.IsDeleted = 0;";

        return await _conn.ExecuteScalarAsync<string?>(sql, new { poId });
    }
    static (int Edit, string? Reason) ComputeEditGate(bool hasGrn, string statusCode)
    {
        if (hasGrn)
            return (2, "GRN exists for this PO. Edit/Amendment  is not allowed.");

        if (statusCode.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            return (0, null);

        if (statusCode.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            return (1, "This PO is approved; editing it will create an amendment and assign a new PO number with a revision code.");

        // default safe fallback = block
        return (0, $"Editing is allowed'.");
    }
    public async Task<(List<GetPOImportPendingGroupDto> Rows, int Total)> GetImportPOPendingAsync(
        int? page, int? size, string? search, int? poId, CancellationToken ct)
    {
        var p = (page.HasValue && page > 0) ? page.Value : 1;
        var s = (size.HasValue && size > 0) ? size.Value : 15;
        var off = (p - 1) * s;
        var like = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var unitId = _ip.GetUnitId();

        // Only “Pending” POs (same as Local)
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

        var sql = @"
            -- 1) Matching PO ids
            CREATE TABLE #filtered (Id INT PRIMARY KEY);
            INSERT INTO #filtered (Id)
            SELECT DISTINCT h.Id
            FROM Purchase.PurchaseOrderHeader h
            JOIN Purchase.PurchaseOrderImportHeader iph ON iph.PurchaseOrderId = h.Id AND iph.IsDeleted = 0
            WHERE h.IsDeleted = 0
              AND (@UnitId IS NULL OR h.UnitId = @UnitId)
              AND h.StatusId = @StatusId
              AND (@PoId IS NULL OR h.Id = @PoId)
              AND (@Search IS NULL OR @Search = '' OR h.PONumber LIKE @LikeSearch);

            -- 2) Page ids
            CREATE TABLE #paged (Id INT PRIMARY KEY);
            WITH numbered AS (
                SELECT Id, ROW_NUMBER() OVER (ORDER BY Id DESC) rn
                FROM #filtered
            )
            INSERT INTO #paged (Id)
            SELECT Id FROM numbered WHERE rn BETWEEN (@off + 1) AND (@off + @size);

            -- 3) Header groups (ImportPOHeader)
            SELECT
                h.Id               AS PurchaseOrderId,
                h.PONumber,
                h.PODate,
                h.VendorId,
                h.PurchaseValue,
                h.StatusId,
                h.ItemTotal,
                h.DiscountTotal,
                h.PandFTotal,
                h.MiscCharges,
                h.GSTTotal,                
                h.CGSTTotal,
                h.SGSTTotal,
                h.IGSTTotal,
                h.FreightTotal,
                h.InsuranceTotal,
                h.TDSTotal,
                h.AdvanceAmount,
                h.CreatedDate      AS createdDate,
                h.CreatedByName    AS createdByName,
                st.Code            AS StatusCode,
                cat.Code           AS POCategoryCode,
                mth.Code           AS POMethodCode
            FROM Purchase.PurchaseOrderHeader h
            LEFT JOIN Purchase.MiscMaster st  ON st.Id  = h.StatusId
            LEFT JOIN Purchase.MiscMaster cat ON cat.Id = h.POCategoryId
            LEFT JOIN Purchase.MiscMaster mth ON mth.Id = h.POMethodId
            WHERE h.Id IN (SELECT Id FROM #paged)
            ORDER BY h.Id DESC;
           
            -- 4) ImportPO details (ImportPODetail)
            SELECT
                d.Id,
                d.PurchaseHeaderId,
                d.IndentId,
                d.ItemId,
                d.ItemSno,
                d.UomId,
                d.UnitPrice,
                d.DutyMasterId,
                d.FreightAmount,
                d.InsuranceAmount,
                d.CIFValue,
                d.BasicCustomDuty,
                d.SocialWelfareSurCharges,
                d.IGST,
                d.IGSTPercentage,
                d.AgriInfraDevCess,
                d.AntiDumpingDuty,
                d.SafeguardDuty,
                d.HealthEducationCess,
                d.OtherCharges,
                d.TotalValue,
                d.GRBasedIV AS GRBasedIV
            FROM Purchase.PurchaseOrderImportDetail d
            JOIN Purchase.PurchaseOrderImportHeader iph ON iph.Id = d.PurchaseHeaderId AND iph.IsDeleted = 0
            WHERE iph.PurchaseOrderId IN (SELECT Id FROM #paged)
            ORDER BY iph.PurchaseOrderId, d.Id;

            -- 4b) Map ImportPOHeaderId -> PurchaseOrderId
            SELECT iph.Id AS ImportPOHeaderId, iph.PurchaseOrderId
            FROM Purchase.PurchaseOrderImportHeader iph
            WHERE iph.IsDeleted = 0
              AND iph.PurchaseOrderId IN (SELECT Id FROM #paged);

            -- 5) Total groups
            SELECT COUNT(1) FROM #filtered;

            -- 6) Cleanup
            DROP TABLE #paged;
            DROP TABLE #filtered;";

        var param = new
        {
            UnitId = unitId,
            StatusId = pending.Id,
            PoId = poId,
            Search = search,
            LikeSearch = like,
            off,
            size = s
        };

        using var multi = await _conn.QueryMultipleAsync(new CommandDefinition(sql, param, cancellationToken: ct));

        // Fetching Headers, Details, and Total
        var headers = (await multi.ReadAsync<GetPOImportPendingGroupDto>()).ToList();
        var details = (await multi.ReadAsync<GetPOImportPendingDto>()).ToList();
        var mapRows = (await multi.ReadAsync<(int ImportHeaderId, int PurchaseOrderId)>()).ToList();
        var poByImportHeaderId = mapRows
            .GroupBy(x => x.ImportHeaderId)
            .ToDictionary(g => g.Key, g => g.First().PurchaseOrderId);

        var total = await multi.ReadFirstAsync<int>();

        // Attach details to headers
        var byPo = headers.ToDictionary(h => h.PurchaseOrderId, h => h);
        foreach (var g in headers) g.Lines ??= new List<GetPOImportPendingDto>();

        foreach (var d in details)
        {
            if (!poByImportHeaderId.TryGetValue(d.PurchaseHeaderId, out var poIdForDetail)) continue;
            if (!byPo.TryGetValue(poIdForDetail, out var grp)) continue;

            grp.Lines.Add(d);
        }
        return (headers, total);
    }
    public async Task<bool> ExistsAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
                SELECT TOP 1 1
                FROM Purchase.PurchaseOrderHeader h
                WHERE h.Id = @Id
                AND h.IsDeleted = 0
                AND h.UnitId = @UnitId;";

        var val = await _conn.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                sql,
                new { Id = poId, UnitId = _ip.GetUnitId() },
                cancellationToken: ct));

        return val.HasValue;
    }
    
}
