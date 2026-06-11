using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Workflow;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.FreightRfq
{
    public class FreightRfqQueryRepository : IFreightRfqQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ISupplierLookup _supplierLookup;
        private readonly ITransporterLookup _transporterLookup;
        private readonly ILocationMasterLookup _locationLookup;
        private readonly IStationLookup _stationLookup;
        private readonly IApprovalRequestRefProvider _approvalRefProvider;

        public FreightRfqQueryRepository(
            IDbConnection dbConnection,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            ISupplierLookup supplierLookup,
            ITransporterLookup transporterLookup,
            ILocationMasterLookup locationLookup,
            IStationLookup stationLookup,
            IApprovalRequestRefProvider approvalRefProvider)
        {
            _dbConnection = dbConnection;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _supplierLookup = supplierLookup;
            _transporterLookup = transporterLookup;
            _locationLookup = locationLookup;
            _stationLookup = stationLookup;
            _approvalRefProvider = approvalRefProvider;
        }

        // Surface the workflow ApprovalRequest id on each row so the approval screen can approve without a second call.
        private async Task EnrichApprovalRequestIdsAsync(IReadOnlyList<FreightRfqListDto> rows)
        {
            if (rows.Count == 0)
                return;

            var ids = rows.Select(r => r.Id).Distinct().ToList();
            var refs = await _approvalRefProvider.GetByModuleAsync(MiscEnumEntity.TransactionTypeFreightRfq, ids);
            if (refs.Count == 0)
                return;

            var map = refs.GroupBy(x => x.ModuleTransactionId)
                          .ToDictionary(g => g.Key, g => g.Max(x => x.ApprovalRequestId));
            foreach (var r in rows)
                if (map.TryGetValue(r.Id, out var arId))
                    r.ApprovalRequestHeaderId = arId;
        }

        public async Task<(List<FreightRfqListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? statusId)
        {
            var searchFilter = string.IsNullOrEmpty(searchTerm)
                ? ""
                : "AND (h.FreightRfqNumber LIKE @Search OR h.SourceStation LIKE @Search OR h.DestinationStation LIKE @Search)";
            var statusFilter = statusId.HasValue ? "AND h.StatusId = @StatusId" : "";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Purchase.FreightRfqHeader h
                WHERE h.IsDeleted = 0 {statusFilter} {searchFilter};

                SELECT h.Id, h.FreightRfqNumber, h.RfqDate,
                       rt.Description AS RfqTypeName,
                       po.PONumber AS PoNumber,
                       CONCAT(h.SourceStation, N' → ', h.DestinationStation) AS Route,
                       (SELECT COUNT(1) FROM Purchase.FreightRfqQuotation q
                        WHERE q.FreightRfqHeaderId = h.Id AND q.IsDeleted = 0) AS QuotesCount,
                       h.ApprovedTransporterId,
                       h.ApprovedFreightValue,
                       h.StatusId, s.Description AS StatusName
                FROM Purchase.FreightRfqHeader h
                LEFT JOIN Purchase.MiscMaster rt ON rt.Id = h.RfqTypeId AND rt.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster s ON s.Id = h.StatusId AND s.IsDeleted = 0
                LEFT JOIN Purchase.RawMaterialPOHeader po ON po.Id = h.PoReferenceId AND po.IsDeleted = 0
                WHERE h.IsDeleted = 0 {statusFilter} {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                StatusId = statusId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<FreightRfqListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any(x => x.ApprovedTransporterId.HasValue))
            {
                var transporters = await _transporterLookup.SearchTransportersAsync(null);
                var dict = transporters.ToDictionary(t => t.Id, t => t.TransporterName);
                foreach (var row in list.Where(x => x.ApprovedTransporterId.HasValue))
                    row.ApprovedTransporterName = dict.GetValueOrDefault(row.ApprovedTransporterId!.Value);
            }

            await EnrichApprovalRequestIdsAsync(list);

            return (list, totalCount);
        }

        public async Task<(List<FreightRfqListDto>, int)> GetPendingAsync(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var query = @"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Purchase.FreightRfqHeader h
                LEFT JOIN Purchase.MiscMaster s ON s.Id = h.StatusId AND s.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND s.Code = 'Pending';

                SELECT h.Id, h.FreightRfqNumber, h.RfqDate,
                       rt.Description AS RfqTypeName,
                       po.PONumber AS PoNumber,
                       CONCAT(h.SourceStation, N' → ', h.DestinationStation) AS Route,
                       (SELECT COUNT(1) FROM Purchase.FreightRfqQuotation q
                        WHERE q.FreightRfqHeaderId = h.Id AND q.IsDeleted = 0) AS QuotesCount,
                       h.ApprovedTransporterId,
                       h.ApprovedFreightValue,
                       h.StatusId, s.Description AS StatusName
                FROM Purchase.FreightRfqHeader h
                LEFT JOIN Purchase.MiscMaster rt ON rt.Id = h.RfqTypeId AND rt.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster s ON s.Id = h.StatusId AND s.IsDeleted = 0
                LEFT JOIN Purchase.RawMaterialPOHeader po ON po.Id = h.PoReferenceId AND po.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND s.Code = 'Pending'
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<FreightRfqListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            await EnrichApprovalRequestIdsAsync(list);

            return (list, totalCount);
        }

        public async Task<FreightRfqDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.FreightRfqNumber, h.RfqDate, h.RfqValidTill, h.RfqTypeId, rt.Description AS RfqTypeName,
                       h.PoReferenceId, po.PONumber AS PoNumber, h.SupplierId,
                       h.SourceLocation, h.SourceStation, h.DestinationLocation, h.DestinationStation,
                       h.TotalQuantity, h.TotalBaleCount, h.StatusId, s.Description AS StatusName,
                       h.SelectedQuotationId, h.ComparisonRemarks,
                       h.ApprovedTransporterId, h.ApprovedRate, h.ApprovedFreightValue, h.ApprovalRemarks,
                       h.IsActive
                FROM Purchase.FreightRfqHeader h
                LEFT JOIN Purchase.MiscMaster rt ON rt.Id = h.RfqTypeId AND rt.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster s ON s.Id = h.StatusId AND s.IsDeleted = 0
                LEFT JOIN Purchase.RawMaterialPOHeader po ON po.Id = h.PoReferenceId AND po.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<FreightRfqDto>(headerSql, new { Id = id });
            if (dto == null)
                return null;

            const string linesSql = @"
                SELECT q.Id, q.FreightRfqHeaderId, q.TransporterId,
                       q.TransportDetailId, q.VehicleNo, q.TransportModeName, q.VehicleTypeName,
                       q.RateBasisId, rb.Description AS RateBasisName,
                       q.QuotedRate, q.NoOfVehicles, q.FreightValue,
                       q.IsSelected, q.IsOverride, q.Remarks
                FROM Purchase.FreightRfqQuotation q
                LEFT JOIN Purchase.MiscMaster rb ON rb.Id = q.RateBasisId AND rb.IsDeleted = 0
                WHERE q.FreightRfqHeaderId = @Id AND q.IsDeleted = 0
                ORDER BY q.Id;";

            dto.Quotations = (await _dbConnection.QueryAsync<FreightRfqQuotationDto>(linesSql, new { Id = id })).ToList();

            if (dto.SupplierId is > 0)
            {
                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(dto.SupplierId.Value);
                dto.SupplierName = supplier?.VendorName;
            }

            if (dto.Quotations.Count > 0 || dto.ApprovedTransporterId.HasValue)
            {
                var transporters = await _transporterLookup.SearchTransportersAsync(null);
                var dict = transporters.ToDictionary(t => t.Id, t => t.TransporterName);

                foreach (var line in dto.Quotations)
                    line.TransporterName = dict.GetValueOrDefault(line.TransporterId);

                if (dto.ApprovedTransporterId.HasValue)
                    dto.ApprovedTransporterName = dict.GetValueOrDefault(dto.ApprovedTransporterId.Value);
            }

            // Derived stats from rows that already carry a quoted rate.
            var rated = dto.Quotations.Where(q => q.QuotedRate.HasValue).ToList();
            if (rated.Count > 0)
            {
                dto.LowestQuotedRate = rated.Min(q => q.QuotedRate);
                dto.HighestQuotedRate = rated.Max(q => q.QuotedRate);

                var freights = rated.Where(q => q.FreightValue.HasValue).Select(q => q.FreightValue!.Value).ToList();
                var lowestFreight = freights.Count > 0 ? freights.Min() : (decimal?)null;
                var selectedFreight = dto.Quotations.FirstOrDefault(q => q.Id == dto.SelectedQuotationId)?.FreightValue;
                if (selectedFreight.HasValue && lowestFreight.HasValue)
                    dto.VarianceAmount = selectedFreight.Value - lowestFreight.Value;
            }

            var approvalRefs = await _approvalRefProvider.GetByModuleAsync(MiscEnumEntity.TransactionTypeFreightRfq, new[] { id });
            if (approvalRefs.Count > 0)
                dto.ApprovalRequestHeaderId = approvalRefs.Max(x => x.ApprovalRequestId);

            return dto;
        }

        public async Task<string> PeekNextNumberAsync(DateTimeOffset rfqDate)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            if (unitId <= 0)
                return string.Empty;

            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeFreightRfqQuotation, MiscEnumEntity.ModulePurchase, unitId);
            if (transactionTypeId is null)
                return string.Empty;

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId.Value);
            return sequences.Count > 0 ? sequences[^1] : string.Empty;
        }

        public async Task<IReadOnlyList<PoReferenceLookupDto>> GetRawMaterialPoReferencesAsync(string? term)
        {
            var searchFilter = string.IsNullOrWhiteSpace(term) ? "" : "AND rmp.PONumber LIKE @Search";

            var sql = $@"
                SELECT rmp.Id, rmp.PONumber AS PoNumber, ISNULL(ocr.SupplierId, 0) AS VendorId
                FROM Purchase.RawMaterialPOHeader rmp
                LEFT JOIN Purchase.OCREntry ocr ON ocr.Id = rmp.OcrId AND ocr.IsDeleted = 0
                WHERE rmp.IsDeleted = 0 {searchFilter}
                ORDER BY rmp.Id DESC;";

            var rows = (await _dbConnection.QueryAsync<PoReferenceLookupDto>(sql, new { Search = $"%{term}%" })).ToList();

            foreach (var supplierId in rows.Where(r => r.VendorId > 0).Select(r => r.VendorId).Distinct())
            {
                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(supplierId);
                if (supplier == null) continue;
                foreach (var r in rows.Where(x => x.VendorId == supplierId))
                    r.VendorName = supplier.VendorName;
            }

            return rows;
        }

        public async Task<FreightRfqPoPrefillDto?> GetPoPrefillAsync(int rawMaterialPoId)
        {
            const string sql = @"
                SELECT rmp.Id AS PoReferenceId, rmp.PONumber,
                       ISNULL(ocr.SupplierId, 0) AS SupplierId,
                       ocr.LocationId, ocr.StationId,
                       (SELECT ISNULL(SUM(d.Quantity), 0) FROM Purchase.RawMaterialPODetail d
                        WHERE d.POHeaderId = rmp.Id AND d.IsDeleted = 0) AS TotalBaleCount,
                       (SELECT ISNULL(SUM(d.Weight), 0) FROM Purchase.RawMaterialPODetail d
                        WHERE d.POHeaderId = rmp.Id AND d.IsDeleted = 0) AS TotalWeightKg
                FROM Purchase.RawMaterialPOHeader rmp
                LEFT JOIN Purchase.OCREntry ocr ON ocr.Id = rmp.OcrId AND ocr.IsDeleted = 0
                WHERE rmp.Id = @Id AND rmp.IsDeleted = 0;";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<PrefillRow>(sql, new { Id = rawMaterialPoId });
            if (row == null)
                return null;

            var dto = new FreightRfqPoPrefillDto
            {
                PoReferenceId = row.PoReferenceId,
                PoNumber = row.PoNumber,
                SupplierId = row.SupplierId,
                TotalBaleCount = (int)Math.Round(row.TotalBaleCount),
                TotalQuantity = Math.Round(row.TotalWeightKg / 1000m, 3)
            };

            if (row.SupplierId > 0)
            {
                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(row.SupplierId);
                dto.SupplierName = supplier?.VendorName;
            }

            if (row.LocationId > 0)
            {
                var loc = await _locationLookup.GetByIdsAsync(new[] { row.LocationId });
                dto.SourceLocation = loc.FirstOrDefault()?.LocationName;
            }

            if (row.StationId > 0)
            {
                var stn = await _stationLookup.GetByIdsAsync(new[] { row.StationId });
                dto.SourceStation = stn.FirstOrDefault()?.StationName;
            }

            return dto;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM Purchase.FreightRfqHeader WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<string?> GetStatusCodeAsync(int id)
        {
            const string query = @"
                SELECT s.Code
                FROM Purchase.FreightRfqHeader h
                LEFT JOIN Purchase.MiscMaster s ON s.Id = h.StatusId
                WHERE h.Id = @Id AND h.IsDeleted = 0;";
            return await _dbConnection.QueryFirstOrDefaultAsync<string>(query, new { Id = id });
        }

        public async Task<int> GetQuotationCountAsync(int id)
        {
            const string query = @"
                SELECT COUNT(1) FROM Purchase.FreightRfqQuotation
                WHERE FreightRfqHeaderId = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
        }

        public async Task<bool> QuotationBelongsToRfqAsync(int rfqId, int quotationId)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.FreightRfqQuotation
                    WHERE Id = @QuotationId AND FreightRfqHeaderId = @RfqId AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { RfqId = rfqId, QuotationId = quotationId });
        }

        public async Task<bool> MiscExistsAsync(int miscMasterId)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.MiscMaster
                    WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { Id = miscMasterId });
        }

        public async Task<bool> PurchaseOrderExistsAsync(int poId)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.RawMaterialPOHeader
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { Id = poId });
        }

        private sealed class PrefillRow
        {
            public int PoReferenceId { get; set; }
            public string? PoNumber { get; set; }
            public int SupplierId { get; set; }
            public int LocationId { get; set; }
            public int StationId { get; set; }
            public decimal TotalBaleCount { get; set; }
            public decimal TotalWeightKg { get; set; }
        }
    }
}
