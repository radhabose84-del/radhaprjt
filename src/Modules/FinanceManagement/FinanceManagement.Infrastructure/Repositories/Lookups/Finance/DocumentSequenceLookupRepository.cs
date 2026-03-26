using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class DocumentSequenceLookupRepository : IDocumentSequenceLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IModuleLookup _moduleLookup;

        public DocumentSequenceLookupRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IFinancialYearLookup financialYearLookup,
            IModuleLookup moduleLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
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

            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.ShortName);

            var years = await _financialYearLookup.GetAllFinancialYearAsync();
            var yearDict = years.ToDictionary(y => y.FinancialYearId, y => y.FinancialYearName);

            var result = new List<string>();
            foreach (var item in rows)
            {
                var unitShortName = unitDict.TryGetValue(item.UnitId, out var u) ? u : null;
                var yearName = yearDict.TryGetValue(item.FinancialYearId, out var y) ? y : null;
                result.Add($"{unitShortName ?? "?"}-{item.TypeShortName ?? "?"}-{yearName ?? "?"}-{item.DocNo.ToString().PadLeft(4, '0')}".ToUpper());
            }

            return result;
        }

        public async Task IncrementDocNoAsync(int transactionTypeId)
        {
            const string sql = @"
                UPDATE [Finance].[DocumentSequence]
                SET DocNo = DocNo + 1
                WHERE TransactionTypeId = @TransactionTypeId AND IsDeleted = 0";

            await _dbConnection.ExecuteAsync(sql, new { TransactionTypeId = transactionTypeId });
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
