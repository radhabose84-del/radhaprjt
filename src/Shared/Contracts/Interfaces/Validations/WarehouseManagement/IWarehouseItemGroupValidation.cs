namespace Contracts.Interfaces.Validations.WarehouseManagement;

public interface IWarehouseItemGroupValidation
{
    Task<bool> HasLinkedItemGroupAsync(int itemGroupId);
    Task<bool> HasActiveItemGroupAsync(int itemGroupId);
}
