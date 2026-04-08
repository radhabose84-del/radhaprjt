namespace Contracts.Interfaces.Validations.PurchaseManagement;

public interface IPurchaseItemValidation
{
    Task<bool> HasLinkedItemAsync(int itemId);
    Task<bool> HasActiveItemAsync(int itemId);
}
