
namespace PartyManagement.Application.Common.Interfaces.IBankMaster;

public interface IBankMasterCommandRepository
{
    Task<int> AddAsync(PartyManagement.Domain.Entities.BankMaster entity, CancellationToken ct);
    Task UpdateAsync(PartyManagement.Domain.Entities.BankMaster entity, CancellationToken ct);
    Task SoftDeleteAsync(PartyManagement.Domain.Entities.BankMaster entity, CancellationToken ct);
}
