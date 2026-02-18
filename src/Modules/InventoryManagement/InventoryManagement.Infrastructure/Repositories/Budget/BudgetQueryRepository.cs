#nullable disable
using System.Globalization;
using InventoryManagement.Application.Budget.Queries.GetAllBudgets;
using InventoryManagement.Application.Budget.Queries.GetBudgetById;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BudgetQueryRepository : IBudgetQueryRepository
    {
        private readonly ApplicationDbContext _context;

        public BudgetQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BudgetResponseDto> GetBudgetByIdAsync(int budgetId)
        {
               var master = await _context.BudgetMaster
                .Include(b => b.BudgetDetail)
                    .ThenInclude(d => d.BudgetLog)
                .Join(_context.ItemCategory.Where(ic => ic.IsBudgetApplicable == 1),
                    bm => bm.BudgetGroupId,
                    ic => ic.Id,
                    (bm, ic) => new { BudgetMaster = bm, ItemCategory = ic })
                .FirstOrDefaultAsync(x => x.BudgetMaster.Id == budgetId);

            if (master == null) return null;

            return new BudgetResponseDto
            {
                Id = master.BudgetMaster.Id,
                UnitId = master.BudgetMaster.UnitId,
                BudgetGroupId = master.BudgetMaster.BudgetGroupId,
                BudgetGroupName = master.ItemCategory.ItemCategoryName,
                FiscalYear = master.BudgetMaster.FiscalYear,
                YearBudgetAmount = master.BudgetMaster.YearBudgetAmount,
                Is_MRApplicable = master.BudgetMaster.Is_MRApplicable,
                Is_POApplicable = master.BudgetMaster.Is_POApplicable,
                Is_ServiceApplicable = master.BudgetMaster.Is_ServiceApplicable,

                Details = master.BudgetMaster.BudgetDetail?.Select(d => new BudgetDetailWithLogsDto
                {
                    DetailId = d.Id,
                    Month = d.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(d.Month), 
                    BudgetAmount = d.BudgetAmount,    
                }).ToList() ?? new()
            };
        }
        public async Task<List<BudgetListDto>> GetAllBudgetsAsync(int? fiscalYear)
        {
            var query = _context.BudgetMaster
            .Include(b => b.BudgetDetail)
            .Join(
                _context.ItemCategory.Where(ic => ic.IsBudgetApplicable == 1),
                bm => bm.BudgetGroupId,
                ic => ic.Id,
                (bm, ic) => new { BudgetMaster = bm, ItemCategory = ic }
            )
            .AsQueryable();
            if (fiscalYear.HasValue)
                query = query.Where(x => x.BudgetMaster.FiscalYear == fiscalYear.Value);

              var result = await query
            .Select(x => new BudgetListDto
            {
                Id = x.BudgetMaster.Id,
                UnitId = x.BudgetMaster.UnitId,
                BudgetGroupId = x.BudgetMaster.BudgetGroupId,
                BudgetGroupName = x.ItemCategory.ItemCategoryName, // ✅ Fetched from join
                FiscalYear = x.BudgetMaster.FiscalYear,
                YearBudgetAmount = x.BudgetMaster.YearBudgetAmount,
                Is_MRApplicable = x.BudgetMaster.Is_MRApplicable,
                Is_POApplicable = x.BudgetMaster.Is_POApplicable,
                Is_ServiceApplicable = x.BudgetMaster.Is_ServiceApplicable,
                Details = x.BudgetMaster.BudgetDetail.Select(d => new BudgetDetailDto
                {
                    DetailId = d.Id,
                    Month = d.Month,
                    BudgetAmount = d.BudgetAmount
                }).ToList()
            })
            .ToListAsync();

        return result;
        }       
    }
}
