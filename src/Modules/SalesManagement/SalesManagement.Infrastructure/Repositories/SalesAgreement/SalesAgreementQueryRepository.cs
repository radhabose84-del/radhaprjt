using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesAgreement
{
    public class SalesAgreementQueryRepository : ISalesAgreementQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICustomerLookup _customerLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IIPAddressService _ipAddressService;

        public SalesAgreementQueryRepository(
            IDbConnection dbConnection,
            ICustomerLookup customerLookup,
            IPaymentTermLookup paymentTermLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IUnitLookup unitLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _customerLookup = customerLookup;
            _paymentTermLookup = paymentTermLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _unitLookup = unitLookup;
            _ipAddressService = ipAddressService;
        }

        // ── Same-module SQL: headers + JOINed StatusName / SalesGroupName ─────
        // Used by GetByIdAsync (no rollups, no derived Status — line-level data available separately).
        private const string HeaderProjection = @"
            SELECT h.Id, h.AgreementNo,
                h.StatusId, sm.Description AS StatusName,
                h.ValidFrom, h.ValidTo,
                h.CustomerId,
                h.SalesGroupId, sg.SalesGroupName,
                h.PaymentTermsId,
                h.Remarks, h.CustomerPoRefno, h.AgentPOAttachment,
                h.UnitId,
                h.IsActive, h.IsDeleted,
                h.CreatedBy, h.CreatedDate, h.CreatedByName,
                h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
            FROM Sales.SalesAgreementHeader h
            LEFT JOIN Sales.MiscMaster sm ON h.StatusId = sm.Id AND sm.IsDeleted = 0
            LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0";

        public async Task<(List<SalesAgreementHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusName)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.AgreementNo LIKE @Search OR h.Remarks LIKE @Search OR sg.SalesGroupName LIKE @Search OR sm.Description LIKE @Search)";

            var statusFilter = string.IsNullOrWhiteSpace(statusName)
                ? ""
                : "AND sm.Description LIKE @StatusName";

            // Unit scoping — auto-filter to the current user's unit. When unit context is absent
            // (system / unscoped caller), @UnitId is NULL and the filter short-circuits to all rows.
            var unitId = _ipAddressService.GetUnitId();

            var parameters = new DynamicParameters();
            parameters.Add("Search", $"%{searchTerm}%");
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            parameters.Add("UnitId", unitId);
            if (!string.IsNullOrWhiteSpace(statusName))
                parameters.Add("StatusName", $"%{statusName}%");

            // Rollup detail quantities at the header level + derive Status from IsActive + ValidTo.
            // Status: IsActive=0 → 'Inactive'; ValidTo < today → 'Expired'; else 'Active'.
            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesAgreementHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm ON h.StatusId = sm.Id AND sm.IsDeleted = 0
                WHERE h.IsDeleted = 0
                  AND (@UnitId IS NULL OR h.UnitId = @UnitId)
                  {searchFilter}
                  {statusFilter};

                SELECT h.Id, h.AgreementNo,
                    h.StatusId, sm.Description AS StatusName,
                    h.ValidFrom, h.ValidTo,
                    h.CustomerId,
                    h.SalesGroupId, sg.SalesGroupName,
                    h.PaymentTermsId,
                    h.Remarks, h.CustomerPoRefno, h.AgentPOAttachment,
                    h.UnitId,
                    ISNULL(d.TotalQty, 0)        AS TotalQty,
                    ISNULL(d.TotalReleasedQty,0) AS TotalReleasedQty,
                    CASE
                        WHEN h.IsActive = 0          THEN 'Inactive'
                        WHEN h.ValidTo < CAST(GETDATE() AS DATE) THEN 'Expired'
                        ELSE 'Active'
                    END                          AS Status,
                    CASE
                        WHEN h.IsActive = 1
                         AND h.ValidTo >= CAST(GETDATE() AS DATE)
                         AND (sm.Code = 'Approved' OR sm.Description = 'Approved')
                         AND (ISNULL(d.TotalQty, 0) - ISNULL(d.TotalReleasedQty, 0)) > 0
                        THEN 'Y' ELSE 'N'
                    END                          AS CancelFlag,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.SalesAgreementHeader h
                LEFT JOIN Sales.MiscMaster sm ON h.StatusId = sm.Id AND sm.IsDeleted = 0
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                OUTER APPLY (
                    SELECT
                        SUM(TotalQty)    AS TotalQty,
                        SUM(ReleasedQty) AS TotalReleasedQty
                    FROM Sales.SalesAgreementDetail
                    WHERE SalesAgreementHeaderId = h.Id
                ) d
                WHERE h.IsDeleted = 0
                  AND (@UnitId IS NULL OR h.UnitId = @UnitId)
                  {searchFilter}
                  {statusFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<SalesAgreementHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count == 0)
                return (list, totalCount);

            // Populate cross-module names on the grid rows (lines not loaded on GetAll for performance)
            await PopulateCrossModuleHeaderNamesAsync(list);

            return (list, totalCount);
        }

        public async Task<SalesAgreementHeaderDto?> GetByIdAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId();

            var headerSql = $@"
                {HeaderProjection}
                WHERE h.Id = @Id
                  AND h.IsDeleted = 0
                  AND (@UnitId IS NULL OR h.UnitId = @UnitId)";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesAgreementHeaderDto>(headerSql, new { Id = id, UnitId = unitId });

            if (header == null)
                return null;

            const string detailSql = @"
                SELECT Id, SalesAgreementHeaderId, ItemId, VariantId, UomId, AgreedRate, TotalQty, ReleasedQty
                FROM Sales.SalesAgreementDetail
                WHERE SalesAgreementHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<SalesAgreementDetailDto>(detailSql, new { HeaderId = id })).ToList();

            await PopulateCrossModuleHeaderNamesAsync(new List<SalesAgreementHeaderDto> { header });
            await PopulateDetailItemNamesAsync(details);

            header.SalesAgreementDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<SalesAgreementLookupDto>> AutocompleteAsync(string term, int? customerId, CancellationToken ct)
        {
            // Eligibility for autocomplete (must satisfy ALL):
            //   • IsActive = 1                              (Inactive  → excluded)
            //   • ValidTo  >= today                         (Expired   → excluded)
            //   • Status   = 'Approved'                     (Pending / Rejected / Cancelled / ForeClosed → excluded)
            //   • BalanceQty (sum totalQty − sum releasedQty) > 0   (no remaining quantity → excluded)
            //   • CustomerId (optional)                     (skipped when @CustomerId IS NULL)
            //   • UnitId    (auto, from JWT/IP)             (skipped when @UnitId IS NULL — unscoped caller)
            var unitId = _ipAddressService.GetUnitId();

            const string sql = @"
                SELECT TOP 50 h.Id, h.AgreementNo, h.CustomerId
                FROM Sales.SalesAgreementHeader h
                LEFT JOIN Sales.MiscMaster sm ON h.StatusId = sm.Id AND sm.IsDeleted = 0
                OUTER APPLY (
                    SELECT
                        SUM(TotalQty)    AS TotalQty,
                        SUM(ReleasedQty) AS TotalReleasedQty
                    FROM Sales.SalesAgreementDetail
                    WHERE SalesAgreementHeaderId = h.Id
                ) d
                WHERE h.IsActive = 1
                  AND h.IsDeleted = 0
                  AND h.ValidTo >= CAST(GETDATE() AS DATE)
                  AND (sm.Code = 'Approved' OR sm.Description = 'Approved')
                  AND (ISNULL(d.TotalQty, 0) - ISNULL(d.TotalReleasedQty, 0)) > 0
                  AND (@CustomerId IS NULL OR h.CustomerId = @CustomerId)
                  AND (@UnitId IS NULL OR h.UnitId = @UnitId)
                  AND (h.AgreementNo LIKE @Term OR CAST(h.Id AS VARCHAR) LIKE @Term)
                ORDER BY h.Id DESC";

            var rows = (await _dbConnection.QueryAsync<AutocompleteRow>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CustomerId = customerId, UnitId = unitId }, cancellationToken: ct))).ToList();

            if (rows.Count == 0)
                return new List<SalesAgreementLookupDto>();

            var customers = await _customerLookup.GetAllCustomerAsync();
            var customerDict = customers.ToDictionary(c => c.Id, c => c.CustomerName);

            return rows.Select(r => new SalesAgreementLookupDto
            {
                Id = r.Id,
                AgreementNo = r.AgreementNo,
                CustomerName = customerDict.TryGetValue(r.CustomerId, out var name) ? name : null
            }).ToList();
        }

        // ── Validation helpers ────────────────────────────────────────────────

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesAgreementHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesGroupExistsAsync(int salesGroupId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesGroup
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesGroupId });
            return count > 0;
        }

        public async Task<bool> StatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.Id = @Id
                  AND mm.IsDeleted = 0
                  AND (mtm.MiscTypeCode = 'ApprovalStatus' OR mtm.Description = 'ApprovalStatus')";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = statusId });
            return count > 0;
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            var customers = await _customerLookup.GetAllCustomerAsync();
            return customers.Any(c => c.Id == customerId);
        }

        public async Task<bool> PaymentTermsExistsAsync(int paymentTermsId)
        {
            var terms = await _paymentTermLookup.GetAllPaymentTermAsync();
            return terms.Any(p => p.Id == paymentTermsId);
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            return items.Any();
        }

        public async Task<bool> VariantExistsAsync(int variantId)
        {
            // Variants live in the same Item table — looked up the same way
            var items = await _itemLookup.GetByIdsAsync(new[] { variantId });
            return items.Any();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task PopulateCrossModuleHeaderNamesAsync(List<SalesAgreementHeaderDto> headers)
        {
            var customers = await _customerLookup.GetAllCustomerAsync();
            var customerDict = customers.ToDictionary(c => c.Id, c => c.CustomerName);

            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);

            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var h in headers)
            {
                h.CustomerName = customerDict.TryGetValue(h.CustomerId, out var cName) ? cName : null;
                h.PaymentTermsName = ptDict.TryGetValue(h.PaymentTermsId, out var ptName) ? ptName : null;

                if (h.UnitId.HasValue && unitDict.TryGetValue(h.UnitId.Value, out var unitName))
                    h.UnitName = unitName;
            }
        }

        private async Task PopulateDetailItemNamesAsync(List<SalesAgreementDetailDto> details)
        {
            if (details.Count == 0)
                return;

            // Items and variants share the same Item table — fetch both in one round-trip
            var variantIds = details.Where(d => d.VariantId.HasValue).Select(d => d.VariantId!.Value);
            var itemIds = details.Select(d => d.ItemId).Concat(variantIds).Distinct().ToList();

            var itemDict = new Dictionary<int, (string ItemCode, string ItemName)>();
            if (itemIds.Count > 0)
            {
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));
            }

            // UOM lookup (cross-module) for the optional UomId column
            var uomIds = details.Where(d => d.UomId.HasValue).Select(d => d.UomId!.Value).Distinct().ToList();
            var uomDict = new Dictionary<int, string>();
            if (uomIds.Count > 0)
            {
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);
            }

            foreach (var d in details)
            {
                if (itemDict.TryGetValue(d.ItemId, out var itemInfo))
                {
                    d.ItemCode = itemInfo.ItemCode;
                    d.ItemName = itemInfo.ItemName;
                }

                if (d.VariantId.HasValue && itemDict.TryGetValue(d.VariantId.Value, out var variantInfo))
                {
                    d.VariantName = variantInfo.ItemName;
                }

                if (d.UomId.HasValue && uomDict.TryGetValue(d.UomId.Value, out var uomName))
                {
                    d.UomName = uomName;
                }
            }
        }

        private sealed class AutocompleteRow
        {
            public int Id { get; set; }
            public string? AgreementNo { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
