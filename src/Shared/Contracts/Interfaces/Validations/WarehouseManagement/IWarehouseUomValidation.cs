namespace Contracts.Interfaces.Validations.WarehouseManagement;

public interface IWarehouseUomValidation
{
    Task<bool> HasLinkedUomAsync(int uomId);
    Task<bool> HasActiveUomAsync(int uomId);
}
