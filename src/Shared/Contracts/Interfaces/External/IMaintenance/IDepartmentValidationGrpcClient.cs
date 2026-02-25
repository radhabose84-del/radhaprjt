namespace Contracts.Interfaces.External.IMaintenance
{
    public interface IDepartmentValidationGrpcClient
    {
        Task<bool> CheckIfDepartmentIsUsedAsync(int departmentId);
    }
}