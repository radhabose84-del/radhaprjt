using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILedgerBalance;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.LedgerBalances
{
    public class LedgerBalanceQueryRepository : ILedgerBalanceQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public LedgerBalanceQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<LedgerBalanceDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, int? companyId,
            int? accountingPeriodId, int? financialYearId, int? glAccountId,
            int? accountTypeId, int? accountGroupId, int? costCentreId, string? searchTerm)
        {
            var where = new List<string>();
            var p = new DynamicParameters();

            if (companyId is > 0) { where.Add("lb.CompanyId = @CompanyId"); p.Add("CompanyId", companyId); }
            if (accountingPeriodId is > 0) { where.Add("lb.AccountingPeriodId = @AccountingPeriodId"); p.Add("AccountingPeriodId", accountingPeriodId); }
            if (financialYearId is > 0) { where.Add("lb.FinancialYearId = @FinancialYearId"); p.Add("FinancialYearId", financialYearId); }
            if (glAccountId is > 0) { where.Add("lb.GlAccountId = @GlAccountId"); p.Add("GlAccountId", glAccountId); }
            if (accountTypeId is > 0) { where.Add("ga.AccountTypeId = @AccountTypeId"); p.Add("AccountTypeId", accountTypeId); }
            if (accountGroupId is > 0) { where.Add("ga.AccountGroupId = @AccountGroupId"); p.Add("AccountGroupId", accountGroupId); }
            if (costCentreId is > 0) { where.Add("lb.CostCentreId = @CostCentreId"); p.Add("CostCentreId", costCentreId); }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                where.Add("(ga.AccountCode LIKE @Search OR ga.AccountName LIKE @Search)");
                p.Add("Search", $"%{searchTerm}%");
            }

            var whereClause = where.Count > 0 ? string.Join(" AND ", where) : "1 = 1";
            p.Add("Offset", (pageNumber - 1) * pageSize);
            p.Add("PageSize", pageSize);

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.LedgerBalance lb
                INNER JOIN Finance.GlAccountMaster ga ON ga.Id = lb.GlAccountId
                WHERE {whereClause};

                SELECT lb.Id, lb.CompanyId,
                    lb.GlAccountId, ga.AccountCode, ga.AccountName,
                    ga.AccountTypeId, act.AccountTypeName,
                    ga.AccountGroupId, ag.GroupCode, ag.GroupName,
                    ag.[Level] AS GroupLevel, ag.IsLeaf, ag.ParentAccountGroupId,
                    pag.GroupCode AS ParentGroupCode, pag.GroupName AS ParentGroupName,
                    lb.AccountingPeriodId, ap.PeriodName,
                    lb.FinancialYearId, lb.CostCentreId, cc.CostCentreName,
                    lb.DrTotal, lb.CrTotal, lb.Balance
                FROM Finance.LedgerBalance lb
                INNER JOIN Finance.GlAccountMaster ga ON ga.Id = lb.GlAccountId
                LEFT JOIN Finance.AccountTypeMaster act ON act.Id = ga.AccountTypeId
                LEFT JOIN Finance.AccountGroup ag ON ag.Id = ga.AccountGroupId
                LEFT JOIN Finance.AccountGroup pag ON pag.Id = ag.ParentAccountGroupId
                LEFT JOIN Finance.AccountingPeriod ap ON ap.Id = lb.AccountingPeriodId
                LEFT JOIN Finance.CostCentre cc ON cc.Id = lb.CostCentreId
                WHERE {whereClause}
                ORDER BY ga.AccountCode ASC, lb.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, p);
            var list = (await multi.ReadAsync<LedgerBalanceDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }
    }
}
