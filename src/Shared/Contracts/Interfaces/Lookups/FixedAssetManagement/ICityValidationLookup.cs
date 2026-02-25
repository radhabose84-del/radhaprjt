namespace Contracts.Interfaces.Lookups.FixedAssetManagement
{
    public interface ICityValidationLookup
    {
        Task<bool> IsCityUsedAsync(int cityId, CancellationToken ct = default);
    }
}
