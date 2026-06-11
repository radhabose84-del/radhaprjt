using System.Data;
using Contracts.Dtos.Lookups.Gate;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Gate;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.QC;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;

namespace PurchaseManagement.Infrastructure.Repositories.Arrival
{
    public sealed class ArrivalQueryRepository : IArrivalQueryRepository
    {
        private readonly IDbConnection _conn;
        private readonly ISupplierLookup _supplierLookup;
        private readonly IStationLookup _stationLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly ITransporterLookup _transporterLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IQcMiscMasterLookup _qcMiscMasterLookup;
        private readonly IQualitySpecificationLookup _qualitySpecificationLookup;
        private readonly IVehicleMovementRecordLookup _vmrLookup;
        private readonly IIPAddressService _ipAddressService;

        public ArrivalQueryRepository(
            IDbConnection conn,
            ISupplierLookup supplierLookup,
            IStationLookup stationLookup,
            IWarehouseLookup warehouseLookup,
            ITransporterLookup transporterLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IPackTypeLookup packTypeLookup,
            IUOMLookup uomLookup,
            IQcMiscMasterLookup qcMiscMasterLookup,
            IQualitySpecificationLookup qualitySpecificationLookup,
            IVehicleMovementRecordLookup vmrLookup,
            IIPAddressService ipAddressService)
        {
            _conn = conn;
            _supplierLookup = supplierLookup;
            _stationLookup = stationLookup;
            _warehouseLookup = warehouseLookup;
            _transporterLookup = transporterLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _packTypeLookup = packTypeLookup;
            _uomLookup = uomLookup;
            _qcMiscMasterLookup = qcMiscMasterLookup;
            _qualitySpecificationLookup = qualitySpecificationLookup;
            _vmrLookup = vmrLookup;
            _ipAddressService = ipAddressService;
        }

        // Current logged-in unit — all arrival/stock queries are scoped to it.
        private int CurrentUnitId() => _ipAddressService.GetUnitId() ?? 0;

        private const string HeaderSelect = @"
            SELECT
                h.Id, h.UnitId, h.ArrivalNumber, h.ArrivalDate,
                h.RawMaterialPOId, po.PONumber,
                h.VehicleNumber, h.SupplierId, h.StationId, h.GodownId, h.TransporterId,
                h.VmrId, h.SupplierLotNo,
                h.FreightRate, h.InvoiceGstNo, h.LrNumber, h.ContainerNo,
                h.LorryIn, h.LorryOut,
                h.GrossWeight, h.TareWeight, h.NetWeight, h.PartyWeight, h.WeightDifference,
                h.MoisturePercentage,
                h.PRFrom, h.PRTo,
                h.QcStatusId,
                h.Remarks, h.IsActive, h.IsDeleted,
                h.CreatedBy, h.CreatedDate, h.CreatedByName,
                h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
            FROM Purchase.ArrivalHeader h
            LEFT JOIN Purchase.RawMaterialPOHeader po ON h.RawMaterialPOId = po.Id AND po.IsDeleted = 0";

        // ArrivalDetail has no IsDeleted column — do NOT filter on it.
        private const string DetailSelect = @"
            SELECT
                d.Id, d.ArrivalHeaderId, d.ItemId, d.HsnId, d.PackTypeId, d.MixCodeId, d.UomId,
                mc.MixCodeDesc,
                d.Rate, d.OrderedQty, d.ArrivedQty, d.CancelledQty, d.BalanceQty,
                d.BatchNumber, d.BaleNumberFrom, d.BaleNumberTo, d.TotalBaleCount
            FROM Purchase.ArrivalDetail d
            LEFT JOIN Purchase.MixCodeMaster mc ON d.MixCodeId = mc.Id AND mc.IsDeleted = 0
            WHERE d.ArrivalHeaderId IN @Ids";

        public async Task<(List<ArrivalDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, bool? pendingStatus = null)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var where = "WHERE h.IsDeleted = 0 AND h.UnitId = @UnitId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                where += " AND (h.ArrivalNumber LIKE @Search OR po.PONumber LIKE @Search OR h.VehicleNumber LIKE @Search)";

            // Pending = QC not yet completed: either no inspection exists (QcStatusId IS NULL) or an
            // inspection exists and is still 'Pending' (QcStatusId = QC.MiscMaster QP_QC_STATUS/'PENDING').
            // Non-pending = disposed (Approved / Rejected — any non-pending QcStatusId).
            int? pendingQcStatusId = null;
            if (pendingStatus.HasValue)
            {
                pendingQcStatusId = await _qcMiscMasterLookup.GetIdByTypeAndCodeAsync("QP_QC_STATUS", "PENDING");
                where += pendingStatus.Value
                    ? " AND (h.QcStatusId IS NULL OR h.QcStatusId = @PendingQcStatusId)"
                    : " AND h.QcStatusId IS NOT NULL AND (@PendingQcStatusId IS NULL OR h.QcStatusId <> @PendingQcStatusId)";
            }

            // When listing pending arrivals: an un-inspected header (QcStatusId IS NULL) only qualifies if
            // a QC inspection actually applies — i.e. at least one detail item (or its category) has an
            // active Qc.QualitySpecification. A header already 'Pending' in QC (inspection exists) is always
            // included. QC applicability is cross-module, so the eligible NULL-status ids are resolved first.
            List<int>? qcApplicableIds = null;
            if (pendingStatus == true)
            {
                qcApplicableIds = await GetQcApplicablePendingHeaderIdsAsync(CurrentUnitId());
                where += " AND (h.QcStatusId = @PendingQcStatusId OR h.Id IN @QcApplicableIds)";
            }

            var sql = $@"
                {HeaderSelect}
                {where}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM Purchase.ArrivalHeader h
                LEFT JOIN Purchase.RawMaterialPOHeader po ON h.RawMaterialPOId = po.Id AND po.IsDeleted = 0
                {where};";

            var args = new
            {
                UnitId = CurrentUnitId(),
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize,
                QcApplicableIds = qcApplicableIds,
                PendingQcStatusId = pendingQcStatusId
            };

            using var multi = await _conn.QueryMultipleAsync(sql, args);
            var items = (await multi.ReadAsync<ArrivalDto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            await LoadDetailsAsync(items);
            await PopulateHeaderNamesAsync(items);
            return (items, total);
        }

        // Pending arrival headers for which a QC inspection applies — i.e. at least one detail item
        // (or its item category) has an active Qc.QualitySpecification. Item category comes from
        // Inventory (IItemLookup) and the spec match from QC (IQualitySpecificationLookup) — both
        // cross-module, so this is resolved here rather than in SQL.
        private async Task<List<int>> GetQcApplicablePendingHeaderIdsAsync(int unitId)
        {
            const string sql = @"
                SELECT h.Id AS HeaderId, d.ItemId AS ItemId
                FROM Purchase.ArrivalHeader h
                JOIN Purchase.ArrivalDetail d ON d.ArrivalHeaderId = h.Id
                WHERE h.IsDeleted = 0 AND h.UnitId = @UnitId AND h.QcStatusId IS NULL;";

            var rows = (await _conn.QueryAsync<HeaderItemRow>(sql, new { UnitId = unitId })).ToList();
            if (rows.Count == 0)
                return new List<int>();

            var itemIds = rows.Select(r => r.ItemId).Where(i => i > 0).Distinct().ToList();
            if (itemIds.Count == 0)
                return new List<int>();

            var itemCategory = (await _itemLookup.GetByIdsAsync(itemIds))
                .ToDictionary(x => x.Id, x => x.ItemCategoryId);
            var categoryIds = itemCategory.Values.Where(c => c > 0).Distinct().ToList();

            var match = await _qualitySpecificationLookup.GetMatchingAsync(itemIds, categoryIds);

            return rows
                .Where(r =>
                    match.MatchedItemIds.Contains(r.ItemId) ||
                    (itemCategory.TryGetValue(r.ItemId, out var cat) && cat > 0 && match.MatchedItemCategoryIds.Contains(cat)))
                .Select(r => r.HeaderId)
                .Distinct()
                .ToList();
        }

        private sealed class HeaderItemRow
        {
            public int HeaderId { get; set; }
            public int ItemId { get; set; }
        }

        public async Task<ArrivalDto?> GetByIdAsync(int id)
        {
            var sql = $"{HeaderSelect} WHERE h.Id = @Id AND h.IsDeleted = 0 AND h.UnitId = @UnitId;";
            var dto = await _conn.QueryFirstOrDefaultAsync<ArrivalDto>(sql, new { Id = id, UnitId = CurrentUnitId() });
            if (dto == null)
                return null;

            var list = new List<ArrivalDto> { dto };
            await LoadDetailsAsync(list);
            await PopulateHeaderNamesAsync(list);
            await LoadIndividualBalesAsync(dto);
            return dto;
        }

        // Attaches the per-bale StockLedgerRaw rows (saved verbatim from the payload) to their detail line.
        // A line with no bale rows is left with an empty Bales list.
        private async Task LoadIndividualBalesAsync(ArrivalDto header)
        {
            if (header.Details.Count == 0)
                return;

            const string sql = @"
                SELECT BaleNo, BarcodeNumber, BaleWeight, ItemId
                FROM Purchase.StockLedgerRaw
                WHERE LotNo = @LotNo AND DocType = 'ARV' AND UnitId = @UnitId
                ORDER BY BaleNo;";

            var rows = (await _conn.QueryAsync(sql, new { LotNo = header.Id, UnitId = CurrentUnitId() })).ToList();
            if (rows.Count == 0)
                return;

            var byItem = rows.GroupBy(r => (int)r.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var detail in header.Details)
            {
                if (!byItem.TryGetValue(detail.ItemId, out var baleRows))
                    continue;

                detail.Bales = baleRows.Select(r => new ArrivalBaleRowDto
                {
                    BaleNo = (long)r.BaleNo,
                    BarcodeNumber = (long?)r.BarcodeNumber,
                    BaleWeight = (decimal)r.BaleWeight
                }).ToList();
            }
        }

        public async Task<IReadOnlyList<ArrivalLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 50 h.Id, h.ArrivalNumber
                FROM Purchase.ArrivalHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0 AND h.UnitId = @UnitId
                  AND (@Term = '' OR h.ArrivalNumber LIKE @Search)
                ORDER BY h.Id DESC;";

            var cmd = new CommandDefinition(sql,
                new { Term = term ?? string.Empty, Search = $"%{term}%", UnitId = CurrentUnitId() }, cancellationToken: ct);
            var rows = await _conn.QueryAsync<ArrivalLookupDto>(cmd);
            return rows.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.ArrivalHeader WHERE Id = @Id AND IsDeleted = 0 AND UnitId = @UnitId;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id, UnitId = CurrentUnitId() });
            return count == 0;
        }

        public async Task<bool> RawMaterialPOExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.RawMaterialPOHeader WHERE Id = @Id AND IsDeleted = 0 AND UnitId = @UnitId;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id, UnitId = CurrentUnitId() });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> BaleRangeOverlapsAsync(long from, long to, int rawMaterialPoId, int? excludeHeaderId)
        {
            // Overlap is scoped to the SAME Raw Material PO (lot) — bale numbers may repeat across lots.
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Purchase.ArrivalDetail d
                    JOIN Purchase.ArrivalHeader h ON d.ArrivalHeaderId = h.Id
                    WHERE h.IsDeleted = 0 AND h.UnitId = @UnitId
                      AND h.RawMaterialPOId = @RawMaterialPOId
                      AND (@ExcludeHeaderId IS NULL OR h.Id <> @ExcludeHeaderId)
                      AND d.BaleNumberFrom <= @To AND d.BaleNumberTo >= @From
                ) THEN 1 ELSE 0 END;";
            var overlaps = await _conn.ExecuteScalarAsync<int>(sql,
                new { From = from, To = to, RawMaterialPOId = rawMaterialPoId, ExcludeHeaderId = excludeHeaderId, UnitId = CurrentUnitId() });
            return overlaps == 1;
        }

        private async Task LoadDetailsAsync(List<ArrivalDto> headers)
        {
            if (headers.Count == 0)
                return;

            var ids = headers.Select(h => h.Id).ToList();
            var details = (await _conn.QueryAsync<ArrivalDetailDto>(DetailSelect, new { Ids = ids })).ToList();

            if (details.Count > 0)
            {
                var distinctItemIds = details.Select(d => d.ItemId).Distinct().ToList();
                var items = await _itemLookup.GetByIdsAsync(distinctItemIds);
                var itemMap = items.ToDictionary(x => x.Id, x => x.ItemName);
                var itemCategoryMap = items.ToDictionary(x => x.Id, x => x.ItemCategoryId);
                var hsnMap = (await _hsnLookup.GetByIdsAsync(details.Select(d => d.HsnId).Distinct()))
                    .ToDictionary(x => x.Id, x => x.HSNCode);
                var packMap = (await _packTypeLookup.GetByIdsAsync(details.Select(d => d.PackTypeId).Distinct()))
                    .ToDictionary(x => x.Id, x => x.PackTypeName);
                var uomMap = (await _uomLookup.GetByIdsAsync(details.Select(d => d.UomId).Distinct()))
                    .ToDictionary(x => x.Id, x => x.UOMName);

                // QC template availability per line — same Y/N rule as GetGrnPendingHeaderQueryHandler:
                // the item (or its item category) has an active Qc.QualitySpecification.
                var categoryIds = itemCategoryMap.Values.Where(c => c > 0).Distinct().ToList();
                var templateMatch = await _qualitySpecificationLookup.GetMatchingAsync(distinctItemIds, categoryIds);

                foreach (var d in details)
                {
                    d.ItemName = itemMap.TryGetValue(d.ItemId, out var itemName) ? itemName : null;
                    d.HsnCode = hsnMap.TryGetValue(d.HsnId, out var hsnCode) ? hsnCode : null;
                    d.PackTypeName = packMap.TryGetValue(d.PackTypeId, out var packName) ? packName : null;
                    d.UomName = uomMap.TryGetValue(d.UomId, out var uomName) ? uomName : null;

                    var itemMatched = templateMatch.MatchedItemIds.Contains(d.ItemId);
                    var categoryMatched = itemCategoryMap.TryGetValue(d.ItemId, out var catId)
                        && catId > 0 && templateMatch.MatchedItemCategoryIds.Contains(catId);
                    d.IsTemplateAvailable = (itemMatched || categoryMatched) ? "Y" : "N";
                }
            }

            var byHeader = details.GroupBy(d => d.ArrivalHeaderId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var header in headers)
                header.Details = byHeader.TryGetValue(header.Id, out var list) ? list : new List<ArrivalDetailDto>();
        }

        private async Task PopulateHeaderNamesAsync(List<ArrivalDto> headers)
        {
            if (headers.Count == 0)
                return;

            var stationMap = (await _stationLookup.GetByIdsAsync(headers.Select(h => h.StationId).Distinct()))
                .ToDictionary(x => x.Id, x => x.StationName);
            var godownMap = (await _warehouseLookup.GetByIdsAsync(headers.Select(h => h.GodownId).Distinct()))
                .ToDictionary(x => x.Id, x => x.WarehouseName);

            // QcStatusId references QC.MiscMaster (cross-module) — resolve via lookup, never a SQL JOIN.
            var qcStatusIds = headers.Where(h => h.QcStatusId.HasValue).Select(h => h.QcStatusId!.Value).Distinct().ToList();
            var qcStatusMap = qcStatusIds.Count > 0
                ? (await _qcMiscMasterLookup.GetByIdsAsync(qcStatusIds)).ToDictionary(x => x.Id, x => x.Description)
                : new Dictionary<int, string?>();

            // VmrId references Gate.VehicleMovementRecord (cross-module) — resolve details via lookup, never a JOIN.
            var vmrIds = headers.Where(h => h.VmrId.HasValue && h.VmrId.Value > 0).Select(h => h.VmrId!.Value).Distinct().ToList();
            var vmrMap = vmrIds.Count > 0
                ? (await _vmrLookup.GetByIdsAsync(vmrIds)).ToDictionary(x => x.Id, x => x)
                : new Dictionary<int, VehicleMovementRecordLookupDto>();

            // QC.MiscMaster Id for QP_SOURCE_TYPE / code 'ARRIVAL' — single value, used to create a QC
            // inspection from an arrival. Resolved once via lookup — no cross-module JOIN.
            var arrivalSourceTypeId = await _qcMiscMasterLookup.GetIdByTypeAndCodeAsync("QP_SOURCE_TYPE", "ARRIVAL");

            foreach (var h in headers)
            {
                h.StationName = stationMap.TryGetValue(h.StationId, out var sName) ? sName : null;
                h.GodownName = godownMap.TryGetValue(h.GodownId, out var gName) ? gName : null;
                h.QcStatusName = h.QcStatusId.HasValue && qcStatusMap.TryGetValue(h.QcStatusId.Value, out var qcName) ? qcName : null;
                h.SourceTypeId = arrivalSourceTypeId;
                h.Vmr = h.VmrId.HasValue && vmrMap.TryGetValue(h.VmrId.Value, out var vmr) ? vmr : null;

                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(h.SupplierId);
                h.SupplierName = supplier?.VendorName;

                var transporter = await _transporterLookup.GetActiveTransporterByIdAsync(h.TransporterId);
                h.TransporterName = transporter?.TransporterName;
            }
        }
    }
}
