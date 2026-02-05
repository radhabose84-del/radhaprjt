using InventoryManagement.Application.Budget.Queries.GetBudgetLogs;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BudgetLogQueryRepository : IBudgetLogQueryRepository
    {
        private readonly ApplicationDbContext _context;

        public BudgetLogQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BudgetLogDto>> GetLogsAsync(int? budgetId, int? budgetDetailId)
        {
             var query = from log in _context.BudgetLog
                join bd in _context.BudgetDetail on log.BudgetDetailId equals bd.Id
                join bm in _context.BudgetMaster on bd.BudgetId equals bm.Id
                join ic in _context.ItemCategory on bm.BudgetGroupId equals ic.Id
                join ig in _context.ItemGroup on ic.ItemGroupId equals ig.Id
                join mm in _context.MiscMaster on log.ActionTypeId equals mm.Id
                where (!budgetDetailId.HasValue || log.BudgetDetailId == budgetDetailId.Value)
                            && (!budgetId.HasValue || bd.BudgetId == budgetId.Value)
                orderby log.CreatedDate descending
                select new BudgetLogDto
                {
                    Id = log.Id,
                    BudgetDetailId = log.BudgetDetailId,
                    ActionTypeId = log.ActionTypeId,
                    OldBudgetAmount = log.OldBudgetAmount,
                    NewBudgetAmount = log.NewBudgetAmount,
                    Remarks = log.Remarks,
                    CreatedDate = log.CreatedDate,
                    CreatedBy = log.CreatedBy,
                    CreatedByName = log.CreatedByName,
                    CreatedIP = log.CreatedIP,

                    // ✅ Additional fields
                    Month = bd.Month,
                    BudgetGroupName = ic.ItemCategoryName,
                    ItemGroupName = ig.ItemGroupName,
                    FiscalYear = bm.FiscalYear,
                    ActionName = mm.Code
                };

            return await query.ToListAsync();
        }
    }
}
