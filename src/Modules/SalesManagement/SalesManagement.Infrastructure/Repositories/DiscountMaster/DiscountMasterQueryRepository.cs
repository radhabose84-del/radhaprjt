using System.Data;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.DiscountMaster
{
    public class DiscountMasterQueryRepository : IDiscountMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public DiscountMasterQueryRepository(
            IDbConnection dbConnection,
            IPaymentTermLookup paymentTermLookup,
            ICurrencyLookup currencyLookup)
        {
            _dbConnection = dbConnection;
            _paymentTermLookup = paymentTermLookup;
            _currencyLookup = currencyLookup;
        }

        public async Task<(List<DiscountMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            const string countSql = @"
                SELECT COUNT(*)
                FROM Sales.DiscountMaster dm
                WHERE dm.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR dm.DiscountName LIKE '%' + @SearchTerm + '%'
                       OR dm.DiscountCode LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT
                    dm.Id, dm.DiscountCode, dm.DiscountName,
                    dm.TriggerEventId, te.Description AS TriggerEventName,
                    dm.DiscountBasisId, db.Description AS DiscountBasisName,
                    dm.ExecutionTypeId, et.Description AS ExecutionTypeName,
                    dm.CurrencyId,
                    dm.CustomerGroupId, cg.Description AS CustomerGroupName,
                    dm.Priority, dm.RequiresApproval,
                    dm.MaxDiscountLimitTypeId, mdl.Description AS MaxDiscountLimitTypeName,
                    dm.MaxDiscountValue, dm.IsStackable,
                    dm.ExclusionGroupId, eg.Description AS ExclusionGroupName,
                    dm.ValueTypeId, vt.Description AS ValueTypeName,
                    dm.SlabTypeId, st.Description AS SlabTypeName,
                    dm.IsActive, dm.IsDeleted,
                    dm.CreatedBy, dm.CreatedDate, dm.CreatedByName, dm.CreatedIP,
                    dm.ModifiedBy, dm.ModifiedDate, dm.ModifiedByName, dm.ModifiedIP
                FROM Sales.DiscountMaster dm
                LEFT JOIN Sales.MiscMaster te ON dm.TriggerEventId = te.Id AND te.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster db ON dm.DiscountBasisId = db.Id AND db.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON dm.ExecutionTypeId = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cg ON dm.CustomerGroupId = cg.Id AND cg.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mdl ON dm.MaxDiscountLimitTypeId = mdl.Id AND mdl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster eg ON dm.ExclusionGroupId = eg.Id AND eg.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster vt ON dm.ValueTypeId = vt.Id AND vt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON dm.SlabTypeId = st.Id AND st.IsDeleted = 0
                WHERE dm.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR dm.DiscountName LIKE '%' + @SearchTerm + '%'
                       OR dm.DiscountCode LIKE '%' + @SearchTerm + '%')
                ORDER BY dm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<DiscountMasterDto>(dataSql, parameters)).ToList();

            // Cross-module: populate CurrencyName via lookup
            var currencyIds = data.Where(d => d.CurrencyId.HasValue).Select(d => d.CurrencyId!.Value).Distinct().ToList();
            if (currencyIds.Count > 0)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds);
                var currDict = currencies.ToDictionary(c => c.CurrencyId, c => c.Name);
                foreach (var item in data.Where(d => d.CurrencyId.HasValue))
                {
                    item.CurrencyName = currDict.TryGetValue(item.CurrencyId!.Value, out var name) ? name : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<DiscountMasterDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    dm.Id, dm.DiscountCode, dm.DiscountName,
                    dm.TriggerEventId, te.Description AS TriggerEventName,
                    dm.DiscountBasisId, db.Description AS DiscountBasisName,
                    dm.ExecutionTypeId, et.Description AS ExecutionTypeName,
                    dm.CurrencyId,
                    dm.CustomerGroupId, cg.Description AS CustomerGroupName,
                    dm.Priority, dm.RequiresApproval,
                    dm.MaxDiscountLimitTypeId, mdl.Description AS MaxDiscountLimitTypeName,
                    dm.MaxDiscountValue, dm.IsStackable,
                    dm.ExclusionGroupId, eg.Description AS ExclusionGroupName,
                    dm.ValueTypeId, vt.Description AS ValueTypeName,
                    dm.SlabTypeId, st.Description AS SlabTypeName,
                    dm.IsActive, dm.IsDeleted,
                    dm.CreatedBy, dm.CreatedDate, dm.CreatedByName, dm.CreatedIP,
                    dm.ModifiedBy, dm.ModifiedDate, dm.ModifiedByName, dm.ModifiedIP
                FROM Sales.DiscountMaster dm
                LEFT JOIN Sales.MiscMaster te ON dm.TriggerEventId = te.Id AND te.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster db ON dm.DiscountBasisId = db.Id AND db.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON dm.ExecutionTypeId = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cg ON dm.CustomerGroupId = cg.Id AND cg.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mdl ON dm.MaxDiscountLimitTypeId = mdl.Id AND mdl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster eg ON dm.ExclusionGroupId = eg.Id AND eg.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster vt ON dm.ValueTypeId = vt.Id AND vt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON dm.SlabTypeId = st.Id AND st.IsDeleted = 0
                WHERE dm.Id = @Id AND dm.IsDeleted = 0";

            const string slabsSql = @"
                SELECT Id, SlabOrder, FromValue, ToValue, DiscountValue
                FROM Sales.DiscountSlab
                WHERE DiscountMasterId = @Id AND IsDeleted = 0
                ORDER BY SlabOrder";

            const string salesGroupsSql = @"
                SELECT dsg.SalesGroupId, sg.SalesGroupName
                FROM Sales.DiscountSalesGroup dsg
                LEFT JOIN Sales.SalesGroup sg ON dsg.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                WHERE dsg.DiscountMasterId = @Id AND dsg.IsDeleted = 0";

            const string paymentTermsSql = @"
                SELECT PaymentTermId
                FROM Sales.DiscountPaymentTerm
                WHERE DiscountMasterId = @Id AND IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<DiscountMasterDto>(headerSql, new { Id = id });

            if (dto == null)
                return null;

            // Cross-module: populate CurrencyName via lookup
            if (dto.CurrencyId.HasValue)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { dto.CurrencyId.Value });
                var currency = currencies.FirstOrDefault();
                dto.CurrencyName = currency?.Name;
            }

            dto.Slabs = (await _dbConnection.QueryAsync<DiscountSlabDto>(slabsSql, new { Id = id })).ToList();
            dto.SalesGroups = (await _dbConnection.QueryAsync<DiscountSalesGroupDto>(salesGroupsSql, new { Id = id })).ToList();

            var paymentTermIds = (await _dbConnection.QueryAsync<int>(paymentTermsSql, new { Id = id })).ToList();

            // Cross-module: populate PaymentTerm descriptions via lookup
            if (paymentTermIds.Count > 0)
            {
                var allPaymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = allPaymentTerms.ToDictionary(pt => pt.Id, pt => pt.Description);

                dto.PaymentTerms = paymentTermIds.Select(ptId => new DiscountPaymentTermDto
                {
                    PaymentTermId = ptId,
                    PaymentTermDescription = ptDict.TryGetValue(ptId, out var desc) ? desc : null
                }).ToList();
            }
            else
            {
                dto.PaymentTerms = new List<DiscountPaymentTermDto>();
            }

            return dto;
        }

        public async Task<IReadOnlyList<DiscountMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, DiscountCode, DiscountName
                FROM Sales.DiscountMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term = '' OR DiscountName LIKE '%' + @Term + '%'
                       OR DiscountCode LIKE '%' + @Term + '%')
                ORDER BY DiscountName ASC";

            var result = await _dbConnection.QueryAsync<DiscountMasterLookupDto>(sql, new { Term = term });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string discountName, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DiscountMaster
                    WHERE DiscountName = @DiscountName AND IsDeleted = 0
                      AND (@Id IS NULL OR Id <> @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { DiscountName = discountName, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Sales.DiscountMaster
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.MiscMaster
                    WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> SalesGroupExistsAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.SalesGroup
                    WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
