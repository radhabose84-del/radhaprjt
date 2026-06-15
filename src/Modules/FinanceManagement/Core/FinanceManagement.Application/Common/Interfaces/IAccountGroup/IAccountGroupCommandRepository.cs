namespace FinanceManagement.Application.Common.Interfaces.IAccountGroup
{
    public interface IAccountGroupCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.AccountGroup entity);
        Task<int> UpdateAsync(Domain.Entities.AccountGroup entity);

        // Re-parents a group (Level is preserved — the validator enforces that the
        // new parent sits exactly one level above the moved group).
        Task<int> MoveAsync(int id, int newParentAccountGroupId);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
