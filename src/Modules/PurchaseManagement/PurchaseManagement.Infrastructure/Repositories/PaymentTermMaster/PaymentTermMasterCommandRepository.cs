using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PaymentTermMaster
{
    public class PaymentTermMasterCommandRepository : IPaymentTermMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PaymentTermMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(PurchaseManagement.Domain.Entities.PaymentTermMaster paymentTermMaster, CancellationToken cancellationToken)
        {
            await _dbContext.PaymentTermMasters.AddAsync(paymentTermMaster, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return paymentTermMaster.Id;
        }
        public async Task<bool> UpdateAsync(PurchaseManagement.Domain.Entities.PaymentTermMaster incoming, List<PaymentTermInstallment>? newInstallments)
        {
            var db = _dbContext;

            var entity = await db.PaymentTermMasters
                .Include(x => x.Installments)
                .FirstOrDefaultAsync(x => x.Id == incoming.Id);

            if (entity == null) return false;

            // update scalars
            entity.Code = incoming.Code.Trim();
            entity.Description = incoming.Description;
            entity.BaselineTypeId = incoming.BaselineTypeId;
            entity.CreditDays = incoming.CreditDays;
            entity.AdvancePercent = incoming.AdvancePercent;
            entity.DiscountPercent = incoming.DiscountPercent;
            entity.DiscountDays = incoming.DiscountDays;
            entity.GraceDays = incoming.GraceDays;
            entity.ApplicableForPortal = incoming.ApplicableForPortal;
            entity.IsActive = incoming.IsActive;

            // replace children
            db.PaymentTermInstallment.RemoveRange(entity.Installments);
            entity.Installments.Clear();

            if (newInstallments != null && newInstallments.Count > 0)
            {
                foreach (var child in newInstallments.OrderBy(c => c.SeqNo))
                {
                    //  attach via navigation; EF will set child.PaymentTermId = entity.Id
                    entity.Installments.Add(child);
                }
            }

            await db.SaveChangesAsync();
            return true;
        }
            
            public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.PaymentTermMasters
                .Include(x => x.Installments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return false;

            if (entity.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted)
                return true; // idempotent

            // mark master
            entity.IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
            entity.IsActive  = PurchaseManagement.Domain.Common.BaseEntity.Status.Inactive;

            // mark children
            foreach (var child in entity.Installments)
            {
                child.IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
                child.IsActive  = PurchaseManagement.Domain.Common.BaseEntity.Status.Inactive;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        //  public async Task<bool> UpdateAsync(PurchaseManagement.Domain.Entities.PaymentTermMaster paymentTermMaster, List<PaymentTermInstallment>? newInstallments                                             )
        // {
        //     var existing = await _dbContext.PaymentTermMasters
        //         .Include(x => x.Installments)
        //         .FirstOrDefaultAsync(x => x.Id == paymentTermMaster.Id);

        //     if (existing == null) return false;

        //     // Update scalars
        //     _dbContext.Entry(existing).CurrentValues.SetValues(paymentTermMaster);
        //     existing.BaselineType = null; // FK-only

        //     // Replace children if provided
        //     if (newInstallments != null)
        //     {
        //         if (existing.Installments?.Count > 0)
        //             _dbContext.PaymentTermInstallment.RemoveRange(existing.Installments);

        //         if (newInstallments.Count > 0)
        //         {
        //             foreach (var ni in newInstallments)
        //             {
        //                 ni.Id = 0;
        //                 ni.PaymentTermId = existing.Id;
        //             }
        //             await _dbContext.PaymentTermInstallment.AddRangeAsync(newInstallments);
        //         }
        //     }

        //     await _dbContext.SaveChangesAsync();
        //     return true;
        // }


    }
}