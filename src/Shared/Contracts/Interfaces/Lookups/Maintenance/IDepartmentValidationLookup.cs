namespace Contracts.Interfaces.Lookups.Maintenance
{
    public interface IDepartmentValidationLookup
    {
        Task<bool> IsDepartmentUsedAsync(int departmentId, CancellationToken ct = default);
    }
}
