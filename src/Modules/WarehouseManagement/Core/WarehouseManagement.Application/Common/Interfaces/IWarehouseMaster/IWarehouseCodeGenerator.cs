namespace WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster
{
    public interface IWarehouseCodeGenerator
    {
        Task<string> GenerateAsync(int unitId, int warehouseTypeId);
         Task<string> RebuildForUpdateAsync(int unitId, int warehouseTypeId, string existingCode, int existingId);
    }
}