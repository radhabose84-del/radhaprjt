using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Infrastructure.Data;

namespace PartyManagement.Infrastructure.Repositories.BankMaster;

public class BankMasterCommandRepository : IBankMasterCommandRepository
{
    private readonly ApplicationDbContext _db;
    public BankMasterCommandRepository(ApplicationDbContext db) => _db = db;

    public async Task<int> AddAsync(PartyManagement.Domain.Entities.BankMaster entity, CancellationToken ct)
    {     
        await _db.BankMaster.AddAsync(entity);            
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(PartyManagement.Domain.Entities.BankMaster entity, CancellationToken ct)
    {
         _db.BankMaster.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(PartyManagement.Domain.Entities.BankMaster entity, CancellationToken ct)
    {
        _db.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
        _db.Entry(entity).Property(e => e.ModifiedBy).IsModified = true;
        _db.Entry(entity).Property(e => e.ModifiedByName).IsModified = true;
        _db.Entry(entity).Property(e => e.ModifiedIP).IsModified = true;
        _db.Entry(entity).Property(e => e.ModifiedDate).IsModified = true;
        await _db.SaveChangesAsync(ct);
    }
}
