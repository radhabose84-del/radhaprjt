using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesItemPriceMaster
{
    public class SalesItemPriceMasterQueryRepository : ISalesItemPriceMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;

        public SalesItemPriceMasterQueryRepository(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            ICurrencyLookup currencyLookup,
            IPaymentTermLookup paymentTermLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _currencyLookup = currencyLookup;
            _paymentTermLookup = paymentTermLookup;
        }

        public async Task<(List<SalesItemPriceMasterDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesItemPriceMaster sipm
                WHERE sipm.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND sipm.PriceCode LIKE @Search")}};

                SELECT
                    sipm.Id, sipm.PriceCode,
                    sipm.ItemId, sipm.SalesSegmentId, sipm.PaymentTermsId,
                    sipm.ExMillPrice, sipm.CurrencyId,
                    sipm.ValidFrom, sipm.ValidTo,
                    sipm.IsActive, sipm.IsDeleted,
                    sipm.CreatedBy, sipm.CreatedDate, sipm.CreatedByName, sipm.CreatedIP,
                    sipm.ModifiedBy, sipm.ModifiedDate, sipm.ModifiedByName, sipm.ModifiedIP,
                    ss.SegmentName AS SalesSegmentName
                FROM Sales.SalesItemPriceMaster sipm
                LEFT JOIN Sales.SalesSegment ss ON sipm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE sipm.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND sipm.PriceCode LIKE @Search")}}
                ORDER BY sipm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<SalesItemPriceMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any())
            {
                var itemIds = list.Select(x => x.ItemId).Distinct();
                var currencyIds = list.Select(x => x.CurrencyId).Distinct();
                var paymentTermIds = list.Select(x => x.PaymentTermsId).Distinct().ToList();

                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(x => x.Id);

                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds);
                var currencyDict = currencies.ToDictionary(x => x.CurrencyId);

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var paymentTermDict = paymentTerms
                    .Where(x => paymentTermIds.Contains(x.Id))
                    .ToDictionary(x => x.Id);

                foreach (var item in list)
                {
                    if (itemDict.TryGetValue(item.ItemId, out var itemData))
                    {
                        item.ItemCode = itemData.ItemCode;
                        item.ItemName = itemData.ItemName;
                    }
                    if (currencyDict.TryGetValue(item.CurrencyId, out var currencyData))
                    {
                        item.CurrencyCode = currencyData.Code;
                    }
                    if (paymentTermDict.TryGetValue(item.PaymentTermsId, out var ptData))
                    {
                        item.PaymentTermsCode = ptData.Code;
                        item.PaymentTermsDescription = ptData.Description;
                    }
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesItemPriceMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    sipm.Id, sipm.PriceCode,
                    sipm.ItemId, sipm.SalesSegmentId, sipm.PaymentTermsId,
                    sipm.ExMillPrice, sipm.CurrencyId,
                    sipm.ValidFrom, sipm.ValidTo,
                    sipm.IsActive, sipm.IsDeleted,
                    sipm.CreatedBy, sipm.CreatedDate, sipm.CreatedByName, sipm.CreatedIP,
                    sipm.ModifiedBy, sipm.ModifiedDate, sipm.ModifiedByName, sipm.ModifiedIP,
                    ss.SegmentName AS SalesSegmentName
                FROM Sales.SalesItemPriceMaster sipm
                LEFT JOIN Sales.SalesSegment ss ON sipm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE sipm.Id = @Id AND sipm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesItemPriceMasterDto>(
                sql, new { Id = id });

            if (dto != null)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId });
                var itemData = items.FirstOrDefault();
                if (itemData != null)
                {
                    dto.ItemCode = itemData.ItemCode;
                    dto.ItemName = itemData.ItemName;
                }

                var currencies = await _currencyLookup.GetByIdsAsync(new[] { dto.CurrencyId });
                var currencyData = currencies.FirstOrDefault();
                if (currencyData != null)
                {
                    dto.CurrencyCode = currencyData.Code;
                }

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptData = paymentTerms.FirstOrDefault(x => x.Id == dto.PaymentTermsId);
                if (ptData != null)
                {
                    dto.PaymentTermsCode = ptData.Code;
                    dto.PaymentTermsDescription = ptData.Description;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<SalesItemPriceMasterLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, PriceCode, ItemId
                FROM Sales.SalesItemPriceMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND PriceCode LIKE @Term
                ORDER BY PriceCode ASC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct)))
                .ToList();

            if (!rows.Any())
                return new List<SalesItemPriceMasterLookupDto>();

            var itemIds = rows.Select(r => (int)r.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
            var itemDict = items.ToDictionary(x => x.Id);

            return rows.Select(r =>
            {
                itemDict.TryGetValue((int)r.ItemId, out var itemData);
                return new SalesItemPriceMasterLookupDto
                {
                    Id = (int)r.Id,
                    PriceCode = (string)r.PriceCode,
                    ItemName = itemData?.ItemName
                };
            }).ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string priceCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.SalesItemPriceMaster
                WHERE PriceCode = @Code AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(
                sql, new { Code = priceCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.SalesItemPriceMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId }, ct);
            return items.Any();
        }

        public async Task<bool> SalesSegmentExistsAsync(int salesSegmentId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.SalesSegment
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesSegmentId });
            return count > 0;
        }

        public async Task<bool> PaymentTermExistsAsync(int paymentTermsId)
        {
            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            return paymentTerms.Any(x => x.Id == paymentTermsId);
        }

        public async Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default)
        {
            var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId }, ct);
            return currencies.Any();
        }

        public async Task<bool> OverlapExistsAsync(
            int itemId, int salesSegmentId, int paymentTermsId,
            DateTimeOffset validFrom, DateTimeOffset validTo, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.SalesItemPriceMaster
                WHERE ItemId = @ItemId
                  AND SalesSegmentId = @SalesSegmentId
                  AND PaymentTermsId = @PaymentTermsId
                  AND IsDeleted = 0
                  AND IsActive = 1
                  AND ValidFrom < @ValidTo
                  AND ValidTo > @ValidFrom";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                ItemId = itemId,
                SalesSegmentId = salesSegmentId,
                PaymentTermsId = paymentTermsId,
                ValidFrom = validFrom,
                ValidTo = validTo,
                ExcludeId = excludeId
            });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Returns true if SalesItemPriceMaster is linked to active dependent records (blocking deletion).
            // Currently SalesItemPriceMaster has no FK children — always returns false (safe to delete).
            await Task.CompletedTask;
            return false;
        }
    }
}
