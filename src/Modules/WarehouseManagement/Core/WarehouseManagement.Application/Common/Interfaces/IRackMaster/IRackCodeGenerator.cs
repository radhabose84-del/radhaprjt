namespace WarehouseManagement.Application.Common.Interfaces.IRackMaster
{
    public interface IRackCodeGenerator
    {
         Task<string> GenerateAsync(int warehouseId, int? floorId, int? aisleId, int? rackLevelId);
    }
}