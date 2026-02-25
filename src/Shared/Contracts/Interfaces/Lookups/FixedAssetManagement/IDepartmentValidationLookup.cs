namespace Contracts.Interfaces.Lookups.FixedAssetManagement
{
    public interface IDepartmentValidationLookup
    {
        Task<bool> IsDepartmentUsedAsync(int departmentId, CancellationToken ct = default);
    }
}
