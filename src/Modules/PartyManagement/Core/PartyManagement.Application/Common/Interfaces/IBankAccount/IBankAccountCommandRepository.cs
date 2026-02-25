namespace PartyManagement.Application.Common.Interfaces.IBankAccount;

public interface IBankAccountCommandRepository
{
    Task<Domain.Entities.BankAccount> AddAsync(PartyManagement.Domain.Entities.BankAccount entity, CancellationToken ct);
    Task UpdateAsync(PartyManagement.Domain.Entities.BankAccount entity, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<PartyManagement.Domain.Entities.BankAccount?> FindAsync(int id, CancellationToken ct);
}