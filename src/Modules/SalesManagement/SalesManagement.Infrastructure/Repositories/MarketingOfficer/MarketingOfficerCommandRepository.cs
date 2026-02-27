using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.MarketingOfficer
{
    public class MarketingOfficerCommandRepository : IMarketingOfficerCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MarketingOfficerCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.MarketingOfficer entity)
        {
            await _dbContext.MarketingOfficer.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.MarketingOfficer entity)
        {
            var existing = await _dbContext.MarketingOfficer
                .Include(x => x.OfficerSalesGroups)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update parent scalar fields
            existing.EmployeeName = entity.EmployeeName;
            existing.MobileNo = entity.MobileNo;
            existing.Email = entity.Email;
            existing.Unit = entity.Unit;
            existing.Department = entity.Department;
            existing.Designation = entity.Designation;
            existing.SalesOfficeId = entity.SalesOfficeId;
            existing.IsActive = entity.IsActive;

            // Full replacement of child records
            if (existing.OfficerSalesGroups != null)
                _dbContext.OfficerSalesGroup.RemoveRange(existing.OfficerSalesGroups);

            foreach (var child in entity.OfficerSalesGroups ?? Enumerable.Empty<Domain.Entities.OfficerSalesGroup>())
            {
                child.MarketingOfficerId = existing.Id;
                await _dbContext.OfficerSalesGroup.AddAsync(child);
            }

            _dbContext.MarketingOfficer.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.MarketingOfficer
                .Include(x => x.OfficerSalesGroups)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;

            foreach (var child in existing.OfficerSalesGroups ?? Enumerable.Empty<Domain.Entities.OfficerSalesGroup>())
            {
                child.IsDeleted = IsDelete.Deleted;
            }

            _dbContext.MarketingOfficer.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
