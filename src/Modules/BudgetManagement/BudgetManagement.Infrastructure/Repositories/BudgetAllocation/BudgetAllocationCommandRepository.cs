using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.Infrastructure.Repositories.BudgetAllocation
{
    public class BudgetAllocationCommandRepository : IBudgetAllocationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public BudgetAllocationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(BudgetManagement.Domain.Entities.BudgetAllocation budgetAllocation)
        {
            budgetAllocation.RemainingBalance= budgetAllocation.ApprovedAmount;
            // Add the BudgetAllocation to the DbContext
            await _applicationDbContext.BudgetAllocations.AddAsync(budgetAllocation);

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();

            // Return the ID of the created BudgetAllocation
            return budgetAllocation.Id;
        }    
        public async Task<bool> UpdateRemainingBalanceAsync(int id, decimal newRemainingBalance, CancellationToken ct = default)
        {            
            var entity = new BudgetManagement.Domain.Entities.BudgetAllocation { Id = id };

            _applicationDbContext.BudgetAllocations.Attach(entity);

            entity.RemainingBalance = newRemainingBalance;

            _applicationDbContext.Entry(entity).Property(x => x.RemainingBalance).IsModified = true;
            
            return await _applicationDbContext.SaveChangesAsync(ct) > 0;
        }
        public async Task<BudgetManagement.Domain.Entities.BudgetAllocation?> GetByKeyAsync(int unitId, int financialYearId, int? requestMonthId, int? budgetGroupId,int requestById,DateOnly fromDate, DateOnly toDate, int? wbsId, int? projectId, CancellationToken ct = default)
        {
            return await _applicationDbContext.BudgetAllocations
            .AsNoTracking()
              .FirstOrDefaultAsync(x =>
                x.UnitId == unitId &&
                x.FinancialYearId == financialYearId &&
                x.BudgetGroupId == budgetGroupId &&
                x.FromDate == fromDate && x.ToDate == toDate && 
                x.ProjectId == projectId && x.WBSId == wbsId &&
                (requestMonthId == null || x.RequestMonthId == requestMonthId)                        
                && x.RequestById == requestById &&
                x.IsDeleted == BaseEntity.IsDelete.NotDeleted,
                ct);

        }      
    }
}