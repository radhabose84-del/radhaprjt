
namespace PartyManagement.Application.Common.Interfaces.IBankMaster;

public interface IBankMasterQueryRepository
{
    Task<(IReadOnlyList<PartyManagement.Domain.Entities.BankMaster> Items, int Total)> GetAllAsync(int page, int size, string? search, CancellationToken ct);
    Task<PartyManagement.Domain.Entities.BankMaster?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsByBankCodeAsync(string name, int? excludeId, CancellationToken ct);
    Task<IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>> GetAutocompleteAsync(string? search, CancellationToken ct);
    Task<string> GenerateBankCodeAsync(string bankName, CancellationToken ct);
    Task<bool> NotFoundAsync(int id);
    Task<bool> SoftDeleteValidationAsync(int id);
    Task<bool> IsBankMasterLinkedAsync(int id);
}
