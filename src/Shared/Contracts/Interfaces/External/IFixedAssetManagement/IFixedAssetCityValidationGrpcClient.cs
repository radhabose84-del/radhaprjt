namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IFixedAssetCityValidationGrpcClient
    {
         Task<bool> CheckIfCityIsUsedForFixedAssetAsync(int cityId);
    }
}