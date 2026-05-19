using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;
using Microsoft.EntityFrameworkCore.Storage;

namespace PurchaseManagement.Infrastructure.Repositories.DutyMaster
{
    public class DutyMasterCommandRepository : IDutyMasterCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public DutyMasterCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService, IDocumentSequenceLookup documentSequenceLookup)
        {
            _db = db;
            _ipAddressService = ipAddressService;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(PurchaseManagement.Domain.Entities.DutyMaster e, int transactionTypeId, CancellationToken ct)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                _db.Add(e);
                await _db.SaveChangesAsync(ct);

                var dbConnection = _db.Database.GetDbConnection();
                var dbTransaction = transaction.GetDbTransaction();
                await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                await transaction.CommitAsync(ct);
                return e.Id;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task UpdateAsync(PurchaseManagement.Domain.Entities.DutyMaster e, CancellationToken ct)
        {
            _db.Update(e);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var e = await _db.Set<PurchaseManagement.Domain.Entities.DutyMaster>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e is null) return false;

            e.IsDeleted = IsDelete.Deleted;
            e.ModifiedBy = _ipAddressService.GetUserId();
            e.ModifiedByName = _ipAddressService.GetUserName();
            e.ModifiedDate = DateTime.UtcNow;
            e.ModifiedIP = _ipAddressService.GetSystemIPAddress();

            await _db.SaveChangesAsync(ct);
            return true;
        }
      
    }
}
