using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Infrastructure.Repositories.ProformaInvoice
{
    public class ProformaInvoiceQueryRepository : IProformaInvoiceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;

        public ProformaInvoiceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
        }

        public async Task<(List<ProformaInvoiceDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (p.ProformaNumber LIKE @Search OR so.SalesOrderNo LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                WHERE p.IsDeleted = 0 {searchFilter};

                SELECT p.Id, p.ProformaNumber, p.ProformaDate,
                    p.SalesOrderId,
                    so.SalesOrderNo,
                    p.PartyId,
                    p.ProformaAmount, p.SOBalance, p.PaymentReceivedAmount,
                    CAST(CASE WHEN p.PaymentReceivedAmount > 0 THEN 1 ELSE 0 END AS BIT) AS PaymentReceivedFlag,
                    p.StatusId,
                    mm.Description AS StatusName,
                    p.Remarks,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE p.IsDeleted = 0 {searchFilter}
                ORDER BY p.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ProformaInvoiceDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<ProformaInvoiceDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.Id, p.ProformaNumber, p.ProformaDate,
                    p.SalesOrderId,
                    so.SalesOrderNo,
                    p.PartyId,
                    p.ProformaAmount, p.SOBalance, p.PaymentReceivedAmount,
                    CAST(CASE WHEN p.PaymentReceivedAmount > 0 THEN 1 ELSE 0 END AS BIT) AS PaymentReceivedFlag,
                    p.StatusId,
                    mm.Description AS StatusName,
                    p.Remarks,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE p.Id = @Id AND p.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ProformaInvoiceDto>(sql, new { Id = id });

            if (dto != null)
            {
                var party = await _partyLookup.GetByIdAsync(dto.PartyId);
                dto.CustomerName = party?.PartyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<ProformaInvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, ProformaNumber, ProformaDate, ProformaAmount
                FROM Sales.ProformaInvoice
                WHERE IsActive = 1 AND IsDeleted = 0
                AND ProformaNumber LIKE @Term
                ORDER BY ProformaNumber ASC";

            var result = await _dbConnection.QueryAsync<ProformaInvoiceLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<List<ProformaInvoiceDto>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT p.Id, p.ProformaNumber, p.ProformaDate,
                    p.SalesOrderId,
                    so.SalesOrderNo,
                    p.PartyId,
                    p.ProformaAmount, p.SOBalance, p.PaymentReceivedAmount,
                    CAST(CASE WHEN p.PaymentReceivedAmount > 0 THEN 1 ELSE 0 END AS BIT) AS PaymentReceivedFlag,
                    p.StatusId,
                    mm.Description AS StatusName,
                    p.Remarks,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE p.SalesOrderId = @SalesOrderId AND p.IsDeleted = 0
                ORDER BY p.Id DESC";

            var list = (await _dbConnection.QueryAsync<ProformaInvoiceDto>(sql, new { SalesOrderId = salesOrderId })).ToList();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                }
            }

            return list;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.ProformaInvoice
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesOrderExistsAndApprovedAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderHeader soh
                    INNER JOIN Sales.MiscMaster mm ON soh.StatusId = mm.Id AND mm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE soh.Id = @Id AND soh.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('ApprovalStatus')
                      AND LOWER(mm.Code) = LOWER('Approved')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<bool> SalesOrderHasAdvancePaymentTypeAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderHeader soh
                    INNER JOIN Sales.MiscMaster mm ON soh.PaymentTypeId = mm.Id AND mm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE soh.Id = @Id AND soh.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('PaymentType')
                      AND LOWER(mm.Code) = LOWER('Advance')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<decimal> GetSalesOrderBalanceAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT ISNULL(soh.FinalAmount, 0) - ISNULL(
                    (SELECT SUM(p.ProformaAmount)
                     FROM Sales.ProformaInvoice p
                     WHERE p.SalesOrderId = @Id AND p.IsDeleted = 0), 0)
                FROM Sales.SalesOrderHeader soh
                WHERE soh.Id = @Id AND soh.IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new { Id = salesOrderId });
        }

        public async Task<bool> IsDraftStatusAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.ProformaInvoice p
                    INNER JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE p.Id = @Id AND p.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('ProformaInvStatus')
                      AND LOWER(mm.Code) = LOWER('Draft')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> StatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.MiscMaster mm
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE mm.Id = @Id AND mm.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('ProformaInvStatus')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = statusId });
        }

        public async Task<decimal> GetProformaAmountAsync(int id)
        {
            const string sql = @"
                SELECT ISNULL(ProformaAmount, 0)
                FROM Sales.ProformaInvoice
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new { Id = id });
        }

        public async Task<bool> HasReceivedAdvancePaymentAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.ProformaInvoice p
                    WHERE p.SalesOrderId = @Id AND p.IsDeleted = 0
                      AND p.PaymentReceivedAmount > 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }
    }
}
