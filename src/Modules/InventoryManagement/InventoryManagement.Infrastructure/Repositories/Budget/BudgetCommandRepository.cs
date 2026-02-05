using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Budget;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
     public class BudgetCommandRepository : BaseQueryRepository, IBudgetCommandRepository
    {
        private readonly ApplicationDbContext _context;

        public BudgetCommandRepository(ApplicationDbContext context, IIPAddressService ipAddressService)
            : base(ipAddressService)
        {
            _context = context;
        }

        #region ✅ Shared Helper - Insert Log
        private async Task InsertBudgetLogAsync(int detailId, string actionCode,
            decimal oldAmount, decimal newAmount,
            string remarks)
        {
            var actionTypeId = await _context.MiscMaster
                .Where(m => m.Code == actionCode)
                .Select(m => m.Id)
                .FirstOrDefaultAsync();

            if (actionTypeId == 0)
                throw new Exception($"Action type '{actionCode}' not found in MiscMaster table.");

            var logEntry = new BudgetLog
            {
                BudgetDetailId = detailId,
                ActionTypeId = actionTypeId,
                OldBudgetAmount = oldAmount,
                NewBudgetAmount = newAmount,
                Remarks = remarks
            };

            _context.BudgetLog.Add(logEntry);
        }
        #endregion

        #region ✅ Create Budget with Logs
        public async Task<int> CreateBudgetAsync(BudgetMaster budgetMaster)
        {
            budgetMaster.UnitId = UnitId;            
            await _context.BudgetMaster.AddAsync(budgetMaster);
            await _context.SaveChangesAsync();

            // Insert log for each detail
            if (budgetMaster.BudgetDetail != null && budgetMaster.BudgetDetail.Any())
            {
                foreach (var detail in budgetMaster.BudgetDetail)
                {
                    await InsertBudgetLogAsync(
                        detail.Id,
                        MiscEnumEntity.Budget_ActionType.Insert,
                        0,
                        detail.BudgetAmount,
                        "Budget detail created"
                    );
                }
                await _context.SaveChangesAsync();
            }

            return budgetMaster.Id;
        }
        #endregion

        #region ✅ Update Master Budget with Log
        public async Task<int> UpdateBudgetMasterAsync(int budgetId, decimal newAmount, string remarks)
        {
            var master = await _context.BudgetMaster.FindAsync(budgetId);
            if (master == null)
                throw new Exception($"Budget master with ID {budgetId} not found.");

            if (master.YearBudgetAmount == newAmount)
                return master.Id; // no change

            var oldAmount = master.YearBudgetAmount;
            master.YearBudgetAmount = newAmount;
           
            // Find first available detail to attach log (or create special master log detail ID = 0)
            var firstDetail = await _context.BudgetDetail
                .Where(d => d.BudgetId == budgetId)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            int logDetailId = firstDetail == 0 ? 0 : firstDetail;

            await InsertBudgetLogAsync(
                logDetailId,
                MiscEnumEntity.Budget_ActionType.Update,
                oldAmount,
                newAmount,
                string.IsNullOrWhiteSpace(remarks) ? "MasterUpdation" : remarks             
            );

            await _context.SaveChangesAsync();
            return master.Id;
        }
        #endregion

        #region ✅ Update Budget Detail with Log
        public async Task<int> UpdateBudgetDetailAsync(int detailId, decimal newAmount, string remarks)
        {
            var detail = await _context.BudgetDetail.FindAsync(detailId);
            if (detail == null)
                throw new Exception($"Budget detail with ID {detailId} not found.");

            if (detail.BudgetAmount == newAmount)
                return detail.Id; // no change

            var oldAmount = detail.BudgetAmount;
            detail.BudgetAmount = newAmount;          

            await InsertBudgetLogAsync(
                detail.Id,
                MiscEnumEntity.Budget_ActionType.Update,
                oldAmount,
                newAmount,
                string.IsNullOrWhiteSpace(remarks) ? "Budget detail updated" : remarks              
            );

            await _context.SaveChangesAsync();
            return detail.Id;
        }
        #endregion

        public async Task<bool> ExistsAsync(int budgetGroupId, int fiscalYear)
        {
            return await _context.BudgetMaster
                .AnyAsync(b => b.UnitId == UnitId && b.BudgetGroupId == budgetGroupId && b.FiscalYear == fiscalYear);
        }
    }
}
