using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.CustomerVisit
{
    public class CustomerVisitCommandRepository : ICustomerVisitCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomerVisitCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CustomerVisit entity)
        {
            await _dbContext.CustomerVisit.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CustomerVisit entity)
        {
            var existing = await _dbContext.CustomerVisit
                .Include(cv => cv.CustomerVisitProducts)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update header fields
            existing.CustomerId = entity.CustomerId;
            existing.VisitTypeId = entity.VisitTypeId;
            existing.VisitDateTime = entity.VisitDateTime;
            existing.Latitude = entity.Latitude;
            existing.Longitude = entity.Longitude;
            existing.ImageName = entity.ImageName;
            existing.Remarks = entity.Remarks;
            existing.MarketingOfficerId = entity.MarketingOfficerId;
            existing.IsActive = entity.IsActive;

            // Replace detail products: remove existing, add new
            if (existing.CustomerVisitProducts != null)
            {
                _dbContext.CustomerVisitProduct.RemoveRange(existing.CustomerVisitProducts);
            }

            if (entity.CustomerVisitProducts != null)
            {
                foreach (var product in entity.CustomerVisitProducts)
                {
                    product.CustomerVisitId = existing.Id;
                    await _dbContext.CustomerVisitProduct.AddAsync(product);
                }
            }

            _dbContext.CustomerVisit.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.CustomerVisit
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.CustomerVisit.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
