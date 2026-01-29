using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style.XmlAccess;

namespace MaintenanceManagement.Infrastructure.Repositories.CostCenter
{
    public class CostCenterCommandRepository : ICostCenterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CostCenterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.CostCenter costCenter)
        {
                // Add the CostCenter to the DbContext
                await _applicationDbContext.CostCenter.AddAsync(costCenter);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created CostCenter
                return costCenter.Id;
        }

        public async Task<int> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.CostCenter costCenter)
        {
            // Fetch the costCenter to delete from the database
            var costCenterToDelete = await _applicationDbContext.CostCenter.FirstOrDefaultAsync(u => u.Id == Id);

            // If the costCenter does not exist
            if (costCenterToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            costCenterToDelete.IsDeleted = costCenter.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<bool> ExistsByCodeAsync(string? costCenterCode)
        {
             if (string.IsNullOrWhiteSpace(costCenterCode))
             return false; // Return false if null/empty
             return await _applicationDbContext.CostCenter.AnyAsync(c => c.CostCenterCode == costCenterCode );
        }

        public async Task<bool> ExistsByCodeOrNameAndUnitAsync(string code, string name, int unitId)
        {
            return await _applicationDbContext.CostCenter
                .AnyAsync(x =>
                    (x.CostCenterCode == code ||
                    x.CostCenterName == name) &&
                    x.UnitId == unitId);
        }

      

        public async Task<int> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.CostCenter costCenter)
        {
         var existingcostcenter = await _applicationDbContext.CostCenter.FirstOrDefaultAsync(u => u.Id == Id);

        // If the costcenter does not exist
        if (existingcostcenter is null)
        {
            return -1; //indicate failure
        }

        // Update the existing costcenter properties
        existingcostcenter.CostCenterName = costCenter.CostCenterName;
        existingcostcenter.UnitId = costCenter.UnitId;
        existingcostcenter.DepartmentId = costCenter.DepartmentId;
        existingcostcenter.EffectiveDate=costCenter.EffectiveDate;
        existingcostcenter.ResponsiblePerson=costCenter.ResponsiblePerson;
        existingcostcenter.BudgetAllocated=costCenter.BudgetAllocated;
        existingcostcenter.Remarks=costCenter.Remarks;
        existingcostcenter.IsActive=costCenter.IsActive;


        // Mark the entity as modified
        _applicationDbContext.CostCenter.Update(existingcostcenter);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }

        public async Task<bool> IsNameDuplicateAsync(string? name, int excludeId, int unitId)
        {
            return await _applicationDbContext.CostCenter
                .AnyAsync(cc => cc.CostCenterName == name && cc.Id != excludeId && cc.UnitId == unitId);
        }
    }
}