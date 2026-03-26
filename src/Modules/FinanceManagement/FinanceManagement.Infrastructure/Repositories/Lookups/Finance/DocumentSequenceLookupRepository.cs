using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class DocumentSequenceLookupRepository : IDocumentSequenceLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IModuleLookup _moduleLookup;

        public DocumentSequenceLookupRepository(
            IDbConnection dbConnection,
            IFinancialYearLookup financialYearLookup,
            IModuleLookup moduleLookup)
        {
            _dbConnection = dbConnection;
            _financialYearLookup = financialYearLookup;
            _moduleLookup = moduleLookup;
        }

        public async Task<int?> GetTransactionTypeIdAsync(string typeName, string moduleName, int unitId)
        {
            var modules = await _moduleLookup.GetAllModuleAsync();
            var module = modules.FirstOrDefault(m => m.ModuleName == moduleName);
            if (module == null)
                return null;

            const string sql = @"
                SELECT Id
                FROM [Finance].[TransactionTypeMaster]
                WHERE TypeName = @TypeName
                  AND ModuleId = @ModuleId
                  AND UnitId = @UnitId
                  AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<int?>(sql, new
            {
                TypeName = typeName,
                ModuleId = module.ModuleId,
                UnitId = unitId
            });
        }

        public async Task<IReadOnlyList<string>> GenerateDocumentNumber(int transactionTypeId)
        {
            const string sql = @"
                SELECT ds.Id, ds.TransactionTypeId, ds.FinancialYearId, ds.DocNo,
                       ttm.ShortName AS TypeShortName, ttm.UnitId
                FROM [Finance].[DocumentSequence] ds
                INNER JOIN [Finance].[TransactionTypeMaster] ttm ON ds.TransactionTypeId = ttm.Id AND ttm.IsDeleted = 0
                WHERE ds.TransactionTypeId = @Id AND ds.IsDeleted = 0
                ORDER BY ds.FinancialYearId, ds.DocNo";

            var rows = (await _dbConnection.QueryAsync<DocSeqRow>(sql, new { Id = transactionTypeId })).ToList();

            if (rows.Count == 0)
                return new List<string>();

            var years = await _financialYearLookup.GetAllFinancialYearAsync();
            var yearDict = years.ToDictionary(y => y.FinancialYearId, y => y.FinancialYearName);

            var result = new List<string>();
            foreach (var item in rows)
            {
                var yearName = yearDict.TryGetValue(item.FinancialYearId, out var y) ? y : null;
                var yearShort = yearName?.Length >= 2 ? yearName[^2..] : yearName ?? "??";
                result.Add($"{item.TypeShortName ?? "?"}/{yearShort}{item.DocNo.ToString().PadLeft(4, '0')}".ToUpper());
            }

            return result;
        }

        public async Task<List<TransactionTypeLookupDto>> GetTransactionTypesForYearAsync(
            int financialYearId, int unitId, int? moduleId = null, int? menuId = null, CancellationToken ct = default)
        {
            var whereClause = @"ttm.IsDeleted = 0 AND ttm.IsActive = 1
                AND ttm.UnitId = @UnitId
                AND ds.FinancialYearId = @FinancialYearId";

            if (moduleId.HasValue && moduleId.Value > 0)
                whereClause += " AND ttm.ModuleId = @ModuleId";

            if (menuId.HasValue && menuId.Value > 0)
                whereClause += " AND ttm.MenuId = @MenuId";

            var sql = $@"
                SELECT DISTINCT ttm.Id, ttm.TypeName, ttm.ShortName, ttm.Description
                FROM [Finance].[TransactionTypeMaster] ttm
                INNER JOIN [Finance].[DocumentSequence] ds ON ds.TransactionTypeId = ttm.Id AND ds.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY ttm.TypeName ASC";

            var result = await _dbConnection.QueryAsync<TransactionTypeLookupDto>(
                new CommandDefinition(sql, new { FinancialYearId = financialYearId, UnitId = unitId, ModuleId = moduleId, MenuId = menuId }, cancellationToken: ct));
            return result.ToList();
        }

   		public async Task IncrementDocNoAsync(int transactionTypeId, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = @"
                UPDATE [Finance].[DocumentSequence]
                SET DocNo = DocNo + 1
                WHERE TransactionTypeId = @TransactionTypeId AND IsDeleted = 0";

            await connection.ExecuteAsync(sql, new { TransactionTypeId = transactionTypeId }, transaction);
        }
        private sealed class DocSeqRow
        {
            public int Id { get; set; }
            public int TransactionTypeId { get; set; }
            public int FinancialYearId { get; set; }
            public int DocNo { get; set; }
            public int UnitId { get; set; }
            public string? TypeShortName { get; set; }
        }
    }
}
