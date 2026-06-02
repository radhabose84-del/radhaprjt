using System.Data;
using Contracts.Dtos.Lookups.QC;
using Contracts.Interfaces.Lookups.QC;
using Dapper;

namespace QCManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Single-query existence check against Qc.QualitySpecification for a batch of ItemIds and/or
    /// ItemCategoryIds. Returns only the dimensions that matched, so callers can decide per-row
    /// availability with HashSet lookups (O(1) per row, one DB hop per request).
    /// </summary>
    internal sealed class QualitySpecificationLookupRepository : IQualitySpecificationLookup
    {
        private readonly IDbConnection _dbConnection;

        public QualitySpecificationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<QualitySpecificationMatchDto> GetMatchingAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> itemCategoryIds,
            CancellationToken ct = default)
        {
            var itemList     = itemIds?.Where(i => i > 0).Distinct().ToList()         ?? new List<int>();
            var categoryList = itemCategoryIds?.Where(c => c > 0).Distinct().ToList() ?? new List<int>();

            if (itemList.Count == 0 && categoryList.Count == 0)
                return new QualitySpecificationMatchDto();

            var orParts    = new List<string>();
            var parameters = new DynamicParameters();

            if (itemList.Count > 0)
            {
                orParts.Add("ItemId IN @ItemIds");
                parameters.Add("ItemIds", itemList);
            }

            if (categoryList.Count > 0)
            {
                orParts.Add("ItemCategoryId IN @CategoryIds");
                parameters.Add("CategoryIds", categoryList);
            }

            var sql = $@"
                SELECT ItemId, ItemCategoryId
                FROM Qc.QualitySpecification
                WHERE IsActive = 1
                  AND IsDeleted = 0
                  AND ({string.Join(" OR ", orParts)});";

            var rows = await _dbConnection.QueryAsync<MatchRow>(
                new CommandDefinition(sql, parameters, cancellationToken: ct));

            var match = new QualitySpecificationMatchDto();
            foreach (var row in rows)
            {
                if (row.ItemId.HasValue && itemList.Contains(row.ItemId.Value))
                    match.MatchedItemIds.Add(row.ItemId.Value);

                if (row.ItemCategoryId.HasValue && categoryList.Contains(row.ItemCategoryId.Value))
                    match.MatchedItemCategoryIds.Add(row.ItemCategoryId.Value);
            }

            return match;
        }

        private sealed class MatchRow
        {
            public int? ItemId { get; set; }
            public int? ItemCategoryId { get; set; }
        }
    }
}
