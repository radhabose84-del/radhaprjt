namespace Contracts.Interfaces.Validations.SalesManagement;

public interface ISalesItemValidation
{
    Task<bool> HasLinkedItemAsync(int itemId);
    Task<bool> HasActiveItemAsync(int itemId);
}
