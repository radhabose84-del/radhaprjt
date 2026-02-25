namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IFixedAssetDepartmentValidationGrpcClient
    {
         Task<bool> CheckIfDepartmentIsUsedForFixedAssetAsync(int departmentId);
    }
}