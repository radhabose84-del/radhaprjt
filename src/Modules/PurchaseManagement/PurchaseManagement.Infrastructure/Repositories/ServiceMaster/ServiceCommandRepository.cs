using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.EventHandlers;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.ServiceMaster
{
    public class ServiceCommandRepository : IServiceCommandRepository
    {
        private readonly ApplicationDbContext _db;




        public ServiceCommandRepository(ApplicationDbContext dbContext)
        {
            _db = dbContext;

        }

        public async Task<PurchaseManagement.Domain.Entities.ServiceMaster> CreateAsync(PurchaseManagement.Domain.Entities.ServiceMaster entity, CancellationToken ct = default)
        {
            // Works with or without SQL Server retry strategy
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                // 1) Insert without ServiceCode to get identity Id
                await _db.Set<PurchaseManagement.Domain.Entities.ServiceMaster>().AddAsync(entity, ct);
                await _db.SaveChangesAsync(ct); // entity.Id is now populated

                // 2) Generate code from Id (SRV0001, SRV0002, … SRV10000, etc.)
                entity.ServiceCode = $"SRV{entity.Id:D4}";

                // 3) Persist the code
                _db.Entry(entity).Property(e => e.ServiceCode).IsModified = true;
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return entity;
            });
        }

       

        public async Task<PurchaseManagement.Domain.Entities.ServiceMaster> UpdateAsync(int id, PurchaseManagement.Domain.Entities.ServiceMaster serviceMaster, CancellationToken ct = default)
        {
            var entity = await _db.Set<PurchaseManagement.Domain.Entities.ServiceMaster>()
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (entity is null)
                throw new KeyNotFoundException($"ServiceMaster {id} not found.");


            entity.ServiceDescription = serviceMaster.ServiceDescription;
            entity.SacId = serviceMaster.SacId;
            entity.UomId = serviceMaster.UomId;
            entity.ServiceCategoryId = serviceMaster.ServiceCategoryId;
            entity.IsActive = serviceMaster.IsActive;


            _db.Entry(entity).Property(e => e.ServiceCode).IsModified = false;

            await _db.SaveChangesAsync(ct);
            return entity;
        }
        
            
        public async Task<bool> SoftDeleteAsync(PurchaseManagement.Domain.Entities.ServiceMaster entity, CancellationToken ct = default)
        {
              var existingservice = await _db.Set<PurchaseManagement.Domain.Entities.ServiceMaster>().FirstOrDefaultAsync(u => u.Id == entity.Id);
            if (existingservice != null)
            {
                existingservice.IsDeleted = entity.IsDeleted;
                existingservice.IsActive= PurchaseManagement.Domain.Common.BaseEntity.Status.Inactive;
                
                return await _db.SaveChangesAsync() > 0;
            }
            return false;
        }


    }   
}