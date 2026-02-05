using PartyManagement.Application.Common;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Data;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Infrastructure.Repositories.BankAccount;

public class BankAccountCommandRepository : IBankAccountCommandRepository
{
    private readonly ApplicationDbContext _db;
    public BankAccountCommandRepository(ApplicationDbContext db) => _db = db;

    public async Task<PartyManagement.Domain.Entities.BankAccount> AddAsync(PartyManagement.Domain.Entities.BankAccount entity, CancellationToken ct)
    {
        _db.BankAccount.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }


    public async Task UpdateAsync(PartyManagement.Domain.Entities.BankAccount entity, CancellationToken ct)
    {
        _db.BankAccount.Update(entity);
        await _db.SaveChangesAsync(ct);
    }


    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.BankAccount   // or _db.BankMasters if that's your DbSet name
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null) return false;

        entity.IsDeleted = IsDelete.Deleted;     
        entity.ModifiedDate = DateTime.UtcNow;
        return await _db.SaveChangesAsync(ct) > 0;
    }


    public Task<PartyManagement.Domain.Entities.BankAccount?> FindAsync(int id, CancellationToken ct)
    => _db.BankAccount.FirstOrDefaultAsync(x => x.Id == id, ct);
}