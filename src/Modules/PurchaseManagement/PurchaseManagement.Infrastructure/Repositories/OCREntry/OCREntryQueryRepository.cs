using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.QC;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.OCREntry
{
    public sealed class OCREntryQueryRepository : IOCREntryQueryRepository
    {
        private readonly IDbConnection _conn;
        private readonly ISupplierLookup _supplierLookup;
        private readonly ILocationMasterLookup _locationLookup;
        private readonly IStationLookup _stationLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ICountMasterLookup _countLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IQualityTemplateLookup _qualityTemplateLookup;

        public OCREntryQueryRepository(
            IDbConnection conn,
            ISupplierLookup supplierLookup,
            ILocationMasterLookup locationLookup,
            IStationLookup stationLookup,
            IItemLookup itemLookup,
            ICountMasterLookup countLookup,
            IUOMLookup uomLookup,
            IQualityTemplateLookup qualityTemplateLookup)
        {
            _conn = conn;
            _supplierLookup = supplierLookup;
            _locationLookup = locationLookup;
            _stationLookup = stationLookup;
            _itemLookup = itemLookup;
            _countLookup = countLookup;
            _uomLookup = uomLookup;
            _qualityTemplateLookup = qualityTemplateLookup;
        }

        private const string BaseSelect = @"
            SELECT
                o.Id, o.OcrNumber, o.OcrDate,
                o.ProcurementSourceId, ps.Description AS ProcurementSourceName,
                o.ProcurementTypeId, pt.Description AS ProcurementTypeName,
                o.BrokerDirectId, bd.Description AS BrokerDirectName,
                o.BrokerName,
                o.GradeId, gr.Description AS GradeName,
                o.PaymentTermId, pm.Description AS PaymentTermName,
                o.StatusId, st.Description AS StatusName,
                o.SupplierId, o.LocationId, o.StationId, o.ItemId, o.CountId,
                o.Quantity, o.Weight, o.Rate, o.ExpectedDispatchDate, o.DocumentPath,
                o.PaymentModeId, pmode.Description AS PaymentModeName,
                o.WeighmentId, wgh.Description AS WeighmentName,
                o.TransitInsuranceId, tins.Description AS TransitInsuranceName,
                o.LorryFreightId, lfr.Description AS LorryFreightName,
                o.ModeOfTransportId, mot.Description AS ModeOfTransportName,
                o.MillSampleNo, o.CottonPassedBy, o.GstPercentage,
                o.DiscountPercentage, o.InsurancePercentage, o.Remarks,
                o.UomId, o.QualityTemplateId,
                o.IsActive, o.IsDeleted,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.RawMaterialPOHeader rp
                    WHERE rp.OcrId = o.Id AND rp.IsDeleted = 0
                ) THEN 1 ELSE 0 END AS bit) AS IsPoCreated,
                o.CreatedBy, o.CreatedDate, o.CreatedByName,
                o.ModifiedBy, o.ModifiedDate, o.ModifiedByName
            FROM Purchase.OCREntry o
            LEFT JOIN Purchase.MiscMaster ps ON o.ProcurementSourceId = ps.Id AND ps.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster pt ON o.ProcurementTypeId = pt.Id AND pt.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster bd ON o.BrokerDirectId = bd.Id AND bd.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster gr ON o.GradeId = gr.Id AND gr.IsDeleted = 0
            LEFT JOIN Purchase.PaymentTermMaster pm ON o.PaymentTermId = pm.Id AND pm.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster st ON o.StatusId = st.Id AND st.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster pmode ON o.PaymentModeId = pmode.Id AND pmode.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster wgh ON o.WeighmentId = wgh.Id AND wgh.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster tins ON o.TransitInsuranceId = tins.Id AND tins.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster lfr ON o.LorryFreightId = lfr.Id AND lfr.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mot ON o.ModeOfTransportId = mot.Id AND mot.IsDeleted = 0";

        public async Task<(List<OCREntryDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var where = "WHERE o.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                where += " AND (o.OcrNumber LIKE @Search OR ps.Description LIKE @Search OR st.Description LIKE @Search)";

            var sql = $@"
                {BaseSelect}
                {where}
                ORDER BY o.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM Purchase.OCREntry o
                LEFT JOIN Purchase.MiscMaster ps ON o.ProcurementSourceId = ps.Id AND ps.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster st ON o.StatusId = st.Id AND st.IsDeleted = 0
                {where};";

            var args = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _conn.QueryMultipleAsync(sql, args);
            var items = (await multi.ReadAsync<OCREntryDto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            await PopulateCrossModuleNamesAsync(items);
            return (items, total);
        }

        public async Task<OCREntryDto?> GetByIdAsync(int id)
        {
            var sql = $"{BaseSelect} WHERE o.Id = @Id AND o.IsDeleted = 0;";
            var dto = await _conn.QueryFirstOrDefaultAsync<OCREntryDto>(sql, new { Id = id });
            if (dto == null)
                return null;

            await PopulateCrossModuleNamesAsync(new List<OCREntryDto> { dto });
            await PopulateQualityParametersAsync(dto);
            return dto;
        }

        private async Task PopulateQualityParametersAsync(OCREntryDto dto)
        {
            const string sql = @"
                SELECT Id, ParamId, Value
                FROM Purchase.OCRQualityParameter
                WHERE OcrId = @Id AND IsDeleted = 0
                ORDER BY Id ASC;";

            var rows = (await _conn.QueryAsync<OCRQualityParameterDto>(sql, new { Id = dto.Id })).ToList();
            dto.QualityParameters = rows;
            if (rows.Count == 0)
                return;

            // Resolve parameter code/name via the QC quality-template lookup.
            var paramLookup = await _qualityTemplateLookup.GetParametersByIdsAsync(
                rows.Select(r => r.ParamId).Distinct());
            var paramMap = paramLookup.ToDictionary(p => p.QualityParameterId);

            foreach (var row in rows)
            {
                if (paramMap.TryGetValue(row.ParamId, out var p))
                {
                    row.ParameterCode = p.ParameterCode;
                    row.ParameterName = p.ParameterName;
                }
            }
        }

        public async Task<(List<OCREntryDto> Items, int Total)> GetPendingAsync(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            const string where = "WHERE o.IsDeleted = 0 AND st.Description = 'Pending'";

            var sql = $@"
                {BaseSelect}
                {where}
                ORDER BY o.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM Purchase.OCREntry o
                LEFT JOIN Purchase.MiscMaster st ON o.StatusId = st.Id AND st.IsDeleted = 0
                {where};";

            var args = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };

            using var multi = await _conn.QueryMultipleAsync(sql, args);
            var items = (await multi.ReadAsync<OCREntryDto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            await PopulateCrossModuleNamesAsync(items);
            return (items, total);
        }

        public async Task<IReadOnlyList<OCREntryLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 50 o.Id, o.OcrNumber
                FROM Purchase.OCREntry o
                WHERE o.IsActive = 1 AND o.IsDeleted = 0
                  AND (@Term = '' OR o.OcrNumber LIKE @Search)
                ORDER BY o.Id DESC;";

            var cmd = new CommandDefinition(sql,
                new { Term = term ?? string.Empty, Search = $"%{term}%" }, cancellationToken: ct);
            var rows = await _conn.QueryAsync<OCREntryLookupDto>(cmd);
            return rows.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.OCREntry WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> IsEditableAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.OCREntry o
                    JOIN Purchase.MiscMaster st ON o.StatusId = st.Id
                    WHERE o.Id = @Id AND o.IsDeleted = 0
                      AND st.Description IN ('Approved', 'Converted')
                ) THEN 0 ELSE 1 END;";
            var editable = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return editable == 1;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // OCR is linked once any non-deleted Raw Material PO references it (Rule #25 — blocks delete).
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.RawMaterialPOHeader
                    WHERE OcrId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _conn.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> PaymentTermExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.PaymentTermMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        private async Task PopulateCrossModuleNamesAsync(List<OCREntryDto> items)
        {
            if (items.Count == 0)
                return;

            var itemMap = (await _itemLookup.GetByIdsAsync(items.Select(x => x.ItemId).Distinct()))
                .ToDictionary(x => x.Id, x => x.ItemName);
            var countMap = (await _countLookup.GetByIdsAsync(items.Select(x => x.CountId).Distinct()))
                .ToDictionary(x => x.Id, x => x.CountDescription);
            var locationMap = (await _locationLookup.GetByIdsAsync(items.Select(x => x.LocationId).Distinct()))
                .ToDictionary(x => x.Id, x => x.LocationName);
            var stationMap = (await _stationLookup.GetByIdsAsync(items.Select(x => x.StationId).Distinct()))
                .ToDictionary(x => x.Id, x => x.StationName);

            // ISupplierLookup has no batch API — resolve distinct suppliers individually.
            var supplierMap = new Dictionary<int, string?>();
            foreach (var supplierId in items.Select(x => x.SupplierId).Distinct())
            {
                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(supplierId);
                supplierMap[supplierId] = supplier?.VendorName;
            }

            // Rate unit name (cross-module Inventory UOM, optional).
            var uomIds = items
                .Where(x => x.UomId.HasValue && x.UomId.Value > 0)
                .Select(x => x.UomId!.Value)
                .Distinct()
                .ToList();
            var uomMap = uomIds.Count > 0
                ? (await _uomLookup.GetByIdsAsync(uomIds)).ToDictionary(x => x.Id, x => x.UOMName)
                : new Dictionary<int, string>();

            // Quality template name (cross-module, optional).
            var templateIds = items
                .Where(x => x.QualityTemplateId.HasValue && x.QualityTemplateId.Value > 0)
                .Select(x => x.QualityTemplateId!.Value)
                .Distinct()
                .ToList();
            var templateMap = templateIds.Count > 0
                ? (await _qualityTemplateLookup.GetByIdsAsync(templateIds)).ToDictionary(x => x.Id, x => x.TemplateName)
                : new Dictionary<int, string?>();

            foreach (var dto in items)
            {
                dto.ItemName = itemMap.TryGetValue(dto.ItemId, out var itemName) ? itemName : null;
                dto.CountName = countMap.TryGetValue(dto.CountId, out var countName) ? countName : null;
                dto.LocationName = locationMap.TryGetValue(dto.LocationId, out var locName) ? locName : null;
                dto.StationName = stationMap.TryGetValue(dto.StationId, out var staName) ? staName : null;
                dto.SupplierName = supplierMap.TryGetValue(dto.SupplierId, out var supName) ? supName : null;
                dto.UomName = dto.UomId.HasValue
                    && uomMap.TryGetValue(dto.UomId.Value, out var uomName) ? uomName : null;
                dto.QualityTemplateName = dto.QualityTemplateId.HasValue
                    && templateMap.TryGetValue(dto.QualityTemplateId.Value, out var tplName) ? tplName : null;
            }
        }
    }
}
