using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.FreightRfq
{
    public class FreightRfqQueryRepository : IFreightRfqQueryRepository
    {
        // Fixed document-type token for freight RFQ numbers (mirrors the command repository).
        private const string RfqPrefixToken = "FRFQ";

        private readonly IDbConnection _dbConnection;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ISupplierLookup _supplierLookup;
        private readonly ITransporterLookup _transporterLookup;

        public FreightRfqQueryRepository(
            IDbConnection dbConnection,
            IFinancialYearLookup financialYearLookup,
            ISupplierLookup supplierLookup,
            ITransporterLookup transporterLookup)
        {
            _dbConnection = dbConnection;
            _financialYearLookup = financialYearLookup;
            _supplierLookup = supplierLookup;
            _transporterLookup = transporterLookup;
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
                LEFT JOIN Purchase.PurchaseOrderHeader po ON po.Id = h.PoReferenceId AND po.IsDeleted = 0
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

            // Enrich approved transporter names (cross-module Party).
            if (list.Any(x => x.ApprovedTransporterId.HasValue))
            {
                var transporters = await _transporterLookup.SearchTransportersAsync(null);
                var dict = transporters.ToDictionary(t => t.Id, t => t.TransporterName);
                foreach (var row in list.Where(x => x.ApprovedTransporterId.HasValue))
                    row.ApprovedTransporterName = dict.GetValueOrDefault(row.ApprovedTransporterId!.Value);
            }

            return (list, totalCount);
        }

        public async Task<(List<FreightRfqListDto>, int)> GetPendingAsync(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            // Pending-approval list for the WorkFlow Approval screen (status Code = 'Pending').
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
                LEFT JOIN Purchase.PurchaseOrderHeader po ON po.Id = h.PoReferenceId AND po.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND s.Code = 'Pending'
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<FreightRfqListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<FreightRfqDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.FreightRfqNumber, h.RfqDate, h.RfqTypeId, rt.Description AS RfqTypeName,
                       h.PoReferenceId, po.PONumber AS PoNumber, h.SupplierId,
                       h.SourceLocation, h.SourceStation, h.DestinationLocation, h.DestinationStation,
                       h.TotalQuantity, h.TotalBaleCount, h.StatusId, s.Description AS StatusName,
                       h.SelectedQuotationId, h.ComparisonRemarks,
                       h.ApprovedTransporterId, h.ApprovedRate, h.ApprovedFreightValue, h.ApprovalRemarks,
                       h.IsActive
                FROM Purchase.FreightRfqHeader h
                LEFT JOIN Purchase.MiscMaster rt ON rt.Id = h.RfqTypeId AND rt.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster s ON s.Id = h.StatusId AND s.IsDeleted = 0
                LEFT JOIN Purchase.PurchaseOrderHeader po ON po.Id = h.PoReferenceId AND po.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<FreightRfqDto>(headerSql, new { Id = id });
            if (dto == null)
                return null;

            const string linesSql = @"
                SELECT q.Id, q.FreightRfqHeaderId, q.TransporterId,
                       q.RateBasisId, rb.Description AS RateBasisName,
                       q.QuotedRate, q.NoOfVehicles, q.FreightValue,
                       q.IsSelected, q.IsOverride, q.Remarks
                FROM Purchase.FreightRfqQuotation q
                LEFT JOIN Purchase.MiscMaster rb ON rb.Id = q.RateBasisId AND rb.IsDeleted = 0
                WHERE q.FreightRfqHeaderId = @Id AND q.IsDeleted = 0
                ORDER BY q.Id;";

            dto.Quotations = (await _dbConnection.QueryAsync<FreightRfqQuotationDto>(linesSql, new { Id = id })).ToList();

            // Supplier name (cross-module Party).
            if (dto.SupplierId is > 0)
            {
                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(dto.SupplierId.Value);
                dto.SupplierName = supplier?.VendorName;
            }

            // Transporter names for quotation rows + approved transporter (cross-module Party).
            if (dto.Quotations.Count > 0 || dto.ApprovedTransporterId.HasValue)
            {
                var transporters = await _transporterLookup.SearchTransportersAsync(null);
                var dict = transporters.ToDictionary(t => t.Id, t => t.TransporterName);

                foreach (var line in dto.Quotations)
                    line.TransporterName = dict.GetValueOrDefault(line.TransporterId);

                if (dto.ApprovedTransporterId.HasValue)
                    dto.ApprovedTransporterName = dict.GetValueOrDefault(dto.ApprovedTransporterId.Value);
            }

            // Derived stats (not stored).
            if (dto.Quotations.Count > 0)
            {
                dto.LowestQuotedRate = dto.Quotations.Min(q => q.QuotedRate);
                dto.HighestQuotedRate = dto.Quotations.Max(q => q.QuotedRate);

                var lowestFreight = dto.Quotations.Min(q => q.FreightValue);
                var selectedFreight = dto.Quotations
                    .FirstOrDefault(q => q.Id == dto.SelectedQuotationId)?.FreightValue;
                if (selectedFreight.HasValue)
                    dto.VarianceAmount = selectedFreight.Value - lowestFreight;
            }

            return dto;
        }

        public async Task<string> PeekNextNumberAsync(DateTimeOffset rfqDate)
        {
            var rfqDay = rfqDate.Date;

            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var financialYear = financialYears
                .FirstOrDefault(f => f.StartDate.Date <= rfqDay && rfqDay <= f.EndDate.Date);

            if (financialYear == null || string.IsNullOrWhiteSpace(financialYear.StartYear))
                return string.Empty;

            var prefix = $"{RfqPrefixToken}-{financialYear.StartYear}-";

            const string query = @"
                SELECT TOP 1 FreightRfqNumber
                FROM Purchase.FreightRfqHeader
                WHERE FreightRfqNumber LIKE @Prefix
                ORDER BY FreightRfqNumber DESC;";

            var lastNumber = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                query, new { Prefix = $"{prefix}%" });

            var nextSerial = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastDash = lastNumber.LastIndexOf('-');
                if (lastDash >= 0 && int.TryParse(lastNumber[(lastDash + 1)..], out var parsed))
                    nextSerial = parsed + 1;
            }

            return $"{prefix}{nextSerial:D4}";
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
                    SELECT 1 FROM Purchase.PurchaseOrderHeader
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { Id = poId });
        }
    }
}
