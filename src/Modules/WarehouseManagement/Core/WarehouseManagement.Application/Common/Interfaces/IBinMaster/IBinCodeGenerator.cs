namespace WarehouseManagement.Application.Common.Interfaces.IBinMaster
{
    public interface IBinCodeGenerator
    {
        Task<string> GenerateAsync(int warehouseId, int? rackId, CancellationToken ct = default);
         
        
    }
}