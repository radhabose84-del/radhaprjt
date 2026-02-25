namespace Contracts.Interfaces.Lookups.FixedAssetManagement
{
    public interface ICountryValidationLookup
    {
        Task<bool> IsCountryUsedAsync(int countryId, CancellationToken ct = default);
    }
}
