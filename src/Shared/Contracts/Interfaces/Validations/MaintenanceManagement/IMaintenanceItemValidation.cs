namespace Contracts.Interfaces.Validations.MaintenanceManagement;

public interface IMaintenanceItemValidation
{
    Task<bool> HasLinkedItemAsync(int itemId);
    Task<bool> HasActiveItemAsync(int itemId);
}
