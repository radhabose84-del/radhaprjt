using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.ItemPriceMaster
{
    public class ItemPriceMasterQueryRepository : IItemPriceMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;

        public ItemPriceMasterQueryRepository(
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

        public async Task<(List<ItemPriceMasterDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.ItemPriceMaster sipm
                WHERE sipm.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND sipm.PriceCode LIKE @Search")}};

                SELECT
                    sipm.Id, sipm.PriceCode,
                    sipm.ItemId, sipm.SalesSegmentId,
                    sipm.BaseRate, sipm.CurrencyId,
                    sipm.ValidFrom, sipm.ValidTo,
                    sipm.StatusId,
                    sm.Description AS StatusName,
                    sipm.IsActive, sipm.IsDeleted,
                    sipm.CreatedBy, sipm.CreatedDate, sipm.CreatedByName, sipm.CreatedIP,
                    sipm.ModifiedBy, sipm.ModifiedDate, sipm.ModifiedByName, sipm.ModifiedIP,
                    ss.SegmentName AS SalesSegmentName
                FROM Sales.ItemPriceMaster sipm
                LEFT JOIN Sales.SalesSegment ss ON sipm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm ON sipm.StatusId = sm.Id AND sm.IsDeleted = 0
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
            var list = (await multi.ReadAsync<ItemPriceMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any())
            {
                var itemIds = list.Select(x => x.ItemId).Distinct();
                var currencyIds = list.Select(x => x.CurrencyId).Distinct();

                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(x => x.Id);

                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds);
                var currencyDict = currencies.ToDictionary(x => x.CurrencyId);

                foreach (var item in list)
                {
                    if (itemDict.TryGetValue(item.ItemId, out var itemData))
                    {
                        item.ItemCode = itemData.ItemCode;
                        item.ItemName = itemData.ItemName;
                        item.VariantName = itemData.ParentItemName;
                    }
                    if (currencyDict.TryGetValue(item.CurrencyId, out var currencyData))
                    {
                        item.CurrencyCode = currencyData.Code;
                    }
                }
            }

            return (list, totalCount);
        }

        public async Task<ItemPriceMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    sipm.Id, sipm.PriceCode,
                    sipm.ItemId, sipm.SalesSegmentId,
                    sipm.BaseRate, sipm.CurrencyId,
                    sipm.ValidFrom, sipm.ValidTo,
                    sipm.StatusId,
                    sm.Description AS StatusName,
                    sipm.IsActive, sipm.IsDeleted,
                    sipm.CreatedBy, sipm.CreatedDate, sipm.CreatedByName, sipm.CreatedIP,
                    sipm.ModifiedBy, sipm.ModifiedDate, sipm.ModifiedByName, sipm.ModifiedIP,
                    ss.SegmentName AS SalesSegmentName
                FROM Sales.ItemPriceMaster sipm
                LEFT JOIN Sales.SalesSegment ss ON sipm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm ON sipm.StatusId = sm.Id AND sm.IsDeleted = 0
                WHERE sipm.Id = @Id AND sipm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ItemPriceMasterDto>(
                sql, new { Id = id });

            if (dto != null)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId });
                var itemData = items.FirstOrDefault();
                if (itemData != null)
                {
                    dto.ItemCode = itemData.ItemCode;
                    dto.ItemName = itemData.ItemName;
                    dto.VariantName = itemData.ParentItemName;
                }

                var currencies = await _currencyLookup.GetByIdsAsync(new[] { dto.CurrencyId });
                var currencyData = currencies.FirstOrDefault();
                if (currencyData != null)
                {
                    dto.CurrencyCode = currencyData.Code;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<ItemPriceMasterLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, PriceCode, ItemId, BaseRate, ValidFrom, ValidTo
                FROM Sales.ItemPriceMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND PriceCode LIKE @Term
                ORDER BY PriceCode ASC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct)))
                .ToList();

            if (!rows.Any())
                return new List<ItemPriceMasterLookupDto>();

            var itemIds = rows.Select(r => (int)r.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
            var itemDict = items.ToDictionary(x => x.Id);

            return rows.Select(r =>
            {
                itemDict.TryGetValue((int)r.ItemId, out var itemData);
                return new ItemPriceMasterLookupDto
                {
                    Id = (int)r.Id,
                    PriceCode = (string)r.PriceCode,
                    ItemName = itemData?.ItemName,
                    BaseRate = (decimal)r.BaseRate,
                    ValidFrom = DateOnly.FromDateTime((DateTime)r.ValidFrom),
                    ValidTo = DateOnly.FromDateTime((DateTime)r.ValidTo)
                };
            }).ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string priceCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.ItemPriceMaster
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
                SELECT COUNT(1) FROM Sales.ItemPriceMaster
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

        public async Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default)
        {
            var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId }, ct);
            return currencies.Any();
        }

        public async Task<bool> OverlapExistsAsync(
            int itemId, int salesSegmentId,
            DateOnly validFrom, DateOnly validTo, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.ItemPriceMaster
                WHERE ItemId = @ItemId
                  AND SalesSegmentId = @SalesSegmentId
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
                ValidFrom = validFrom,
                ValidTo = validTo,
                ExcludeId = excludeId
            });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Returns true if ItemPriceMaster is linked to active dependent records (blocking deletion).
            // Currently ItemPriceMaster has no FK children — always returns false (safe to delete).
            await Task.CompletedTask;
            return false;
        }

        public async Task<int> GetNextPriceCodeSerialAsync(string prefix)
        {
            const string sql = @"
                SELECT ISNULL(MAX(
                    CASE WHEN ISNUMERIC(RIGHT(PriceCode, LEN(PriceCode) - LEN(@Prefix) - 1)) = 1
                         THEN CAST(RIGHT(PriceCode, LEN(PriceCode) - LEN(@Prefix) - 1) AS INT)
                         ELSE 0
                    END), 0)
                FROM Sales.ItemPriceMaster
                WHERE PriceCode LIKE @Pattern AND IsDeleted = 0";

            var maxSerial = await _dbConnection.ExecuteScalarAsync<int>(
                sql, new { Prefix = prefix, Pattern = $"{prefix}-%" });

            return maxSerial + 1;
        }

        public async Task<List<ItemPriceMasterDto>> GetByItemAndDateAsync(int itemId, DateOnly date)
        {
            const string sql = @"
                SELECT
                    sipm.Id, sipm.PriceCode,
                    sipm.ItemId, sipm.SalesSegmentId,
                    sipm.BaseRate, sipm.CurrencyId,
                    sipm.ValidFrom, sipm.ValidTo,
                    sipm.StatusId,
                    sm.Description AS StatusName,
                    sipm.IsActive, sipm.IsDeleted,
                    sipm.CreatedBy, sipm.CreatedDate, sipm.CreatedByName, sipm.CreatedIP,
                    sipm.ModifiedBy, sipm.ModifiedDate, sipm.ModifiedByName, sipm.ModifiedIP,
                    ss.SegmentName AS SalesSegmentName
                FROM Sales.ItemPriceMaster sipm
                LEFT JOIN Sales.SalesSegment ss ON sipm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm ON sipm.StatusId = sm.Id AND sm.IsDeleted = 0
                WHERE sipm.ItemId = @ItemId
                  AND sipm.IsDeleted = 0
                  AND sipm.IsActive = 1
                  AND sipm.ValidFrom <= @Date
                  AND sipm.ValidTo >= @Date
                ORDER BY sipm.BaseRate ASC";

            var list = (await _dbConnection.QueryAsync<ItemPriceMasterDto>(
                sql, new { ItemId = itemId, Date = date })).ToList();

            if (list.Any())
            {
                var currencyIds = list.Select(x => x.CurrencyId).Distinct();

                var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
                var itemData = items.FirstOrDefault();

                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds);
                var currencyDict = currencies.ToDictionary(x => x.CurrencyId);

                foreach (var item in list)
                {
                    if (itemData != null)
                    {
                        item.ItemCode = itemData.ItemCode;
                        item.ItemName = itemData.ItemName;
                        item.VariantName = itemData.ParentItemName;
                    }
                    if (currencyDict.TryGetValue(item.CurrencyId, out var currencyData))
                    {
                        item.CurrencyCode = currencyData.Code;
                    }
                }
            }

            return list;
        }

        public async Task<bool> IsItemPriceMasterPendingAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.ItemPriceMaster ipm
                INNER JOIN Sales.MiscMaster mm ON ipm.StatusId = mm.Id AND mm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE ipm.Id = @Id AND ipm.IsDeleted = 0
                  AND mt.Description = @MiscType
                  AND mm.Code = @StatusCode";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                Id = id,
                MiscType = MiscEnumEntity.InvoiceApprovalStatus,
                StatusCode = MiscEnumEntity.InvoiceStatusPending
            });
            return count > 0;
        }

        public async Task<List<ExMillRateDto>> GetExMillRateByPaymentTermAsync(int paymentTermId, int itemId, int? salesSegmentId = null)
        {
            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            var paymentTerm = paymentTerms.FirstOrDefault(pt => pt.Id == paymentTermId);

            if (paymentTerm == null)
                return [];

            const string sql = @"
                SELECT
                    sipm.Id, sipm.PriceCode,
                    sipm.ItemId, sipm.SalesSegmentId,
                    sipm.BaseRate,
                    ss.SegmentName AS SalesSegmentName
                FROM Sales.ItemPriceMaster sipm
                LEFT JOIN Sales.SalesSegment ss ON sipm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE sipm.IsDeleted = 0 AND sipm.IsActive = 1
                  AND sipm.ItemId = @ItemId
                  AND (@SalesSegmentId IS NULL OR sipm.SalesSegmentId = @SalesSegmentId)
                ORDER BY sipm.PriceCode ASC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(sql, new { ItemId = itemId, SalesSegmentId = salesSegmentId })).ToList();

            if (!rows.Any())
                return [];

            return rows.Select(r =>
            {
                var baseRate = (decimal)r.BaseRate;
                var calculated = baseRate + paymentTerm.AdditionalValue;
                return new ExMillRateDto
                {
                    Id = (int)r.Id,
                    PriceCode = (string?)r.PriceCode,
                    SalesSegmentId = (int)r.SalesSegmentId,
                    SalesSegmentName = (string?)r.SalesSegmentName,
                    ExMillRate = calculated != 0 ? calculated : baseRate
                };
            }).ToList();
        }
    }
}
