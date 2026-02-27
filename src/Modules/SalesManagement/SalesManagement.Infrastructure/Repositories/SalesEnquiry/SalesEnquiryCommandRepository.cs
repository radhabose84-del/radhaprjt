using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesEnquiry
{
    public class SalesEnquiryCommandRepository : ISalesEnquiryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesEnquiryCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(SalesEnquiryHeader entity)
        {
            await _applicationDbContext.SalesEnquiryHeader.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(SalesEnquiryHeader entity)
        {
            var existingEntity = await _applicationDbContext.SalesEnquiryHeader
                .Include(h => h.SalesEnquiryDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.PartyId = entity.PartyId;
            existingEntity.EnquiryDate = entity.EnquiryDate;
            existingEntity.ContactPerson = entity.ContactPerson;
            existingEntity.ExpectedDeliveryDate = entity.ExpectedDeliveryDate;
            existingEntity.PaymentTermId = entity.PaymentTermId;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.IsActive = entity.IsActive;

            // Remove existing details and replace with new ones
            if (existingEntity.SalesEnquiryDetails != null && existingEntity.SalesEnquiryDetails.Any())
            {
                _applicationDbContext.SalesEnquiryDetail.RemoveRange(existingEntity.SalesEnquiryDetails);
            }

            if (entity.SalesEnquiryDetails != null && entity.SalesEnquiryDetails.Any())
            {
                foreach (var detail in entity.SalesEnquiryDetails)
                {
                    detail.SalesEnquiryHeaderId = existingEntity.Id;
                    await _applicationDbContext.SalesEnquiryDetail.AddAsync(detail);
                }
            }

            _applicationDbContext.SalesEnquiryHeader.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesEnquiryHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesEnquiryHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
