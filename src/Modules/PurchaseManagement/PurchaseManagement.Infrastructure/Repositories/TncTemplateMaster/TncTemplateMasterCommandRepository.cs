#nullable disable
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster
{
    public class TncTemplateMasterCommandRepository : ITnCTemplateMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TncTemplateMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<int> CreateAsync(TnCTemplateMaster entity, CancellationToken ct)
        {

            _dbContext.TnCTemplateMaster.Add(entity); // navigation children included
            await _dbContext.SaveChangesAsync(ct);    // EF wraps this in a transaction
            return entity.Id;
        }
        public async Task<bool> UpdateAsync(TnCTemplateMaster incoming, List<TnCTemplateApplicability> newApplicabilities)
        {
            var db = _dbContext;

            // Load master + children (tracked)
            var entity = await db.Set<TnCTemplateMaster>()
                .Include(x => x.Applicabilities)
                .FirstOrDefaultAsync(x => x.Id == incoming.Id && x.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            if (entity == null) return false;

            // -------- update scalars --------
            //  entity.TemplateCode   = incoming.TemplateCode;
            entity.TemplateName = incoming.TemplateName?.Trim();
            entity.ModuleId = incoming.ModuleId;
            entity.TermsHtml = incoming.TermsHtml;
            entity.IsActive= incoming.IsActive;



            // -------- replace children (remove all then add new) --------
            if (entity.Applicabilities is not null && entity.Applicabilities.Count > 0)
            {
                db.Set<TnCTemplateApplicability>().RemoveRange(entity.Applicabilities);
                entity.Applicabilities.Clear();
            }

            if (newApplicabilities != null && newApplicabilities.Count > 0)
            {
                foreach (var child in newApplicabilities.OrderBy(c => c.TransactionTypeId))
                {
                    // fresh instance; EF will set FK via navigation
                    child.Id = 0;
                    child.IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active;
                    child.IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted;


                    entity.Applicabilities.Add(child);
                }
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException s)
            {
                switch (s.Number)
                {
                    case 2601: // dup index (e.g., UX_TnC_Module_Name or UX_TnC_Template_TxnType)
                    case 2627: // unique constraint
                        throw new ExceptionRules("A template with this name already exists for the selected Module or there is a duplicate Transaction Type.");
                    case 547:  // FK violation
                        throw new ExceptionRules("Invalid reference. One of the selected Transaction Types or the Module does not exist.");
                    default:
                        throw;
                }
            }

            return true;
        }
        
        // TnCTemplateMasterCommandRepository.cs
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _dbContext.Set<TnCTemplateMaster>()
                .Include(x => x.Applicabilities)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            if (entity == null) return false;

            // mark master
            entity.IsDeleted   = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
            entity.IsActive    = PurchaseManagement.Domain.Common.BaseEntity.Status.Inactive;
           

            // also mark children
            if (entity.Applicabilities != null)
            {
                foreach (var child in entity.Applicabilities)
                {
                    child.IsDeleted   = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
                    child.IsActive    = PurchaseManagement.Domain.Common.BaseEntity.Status.Inactive;
                   
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        
       
    }
}