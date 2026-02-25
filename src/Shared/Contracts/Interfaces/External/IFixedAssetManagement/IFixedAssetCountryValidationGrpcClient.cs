namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IFixedAssetCountryValidationGrpcClient
    {
        Task<bool> CheckIfCountryIsUsedForFixedAssetAsync(int countryId);
    }
}