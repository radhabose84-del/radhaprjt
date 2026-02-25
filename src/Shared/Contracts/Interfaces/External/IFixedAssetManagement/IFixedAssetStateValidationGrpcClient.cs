namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IFixedAssetStateValidationGrpcClient
    {
         Task<bool> CheckIfStateIsUsedForFixedAssetAsync(int cityId);
    }
}