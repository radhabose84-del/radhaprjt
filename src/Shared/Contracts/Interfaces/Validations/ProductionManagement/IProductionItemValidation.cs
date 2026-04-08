namespace Contracts.Interfaces.Validations.ProductionManagement;

public interface IProductionItemValidation
{
    Task<bool> HasLinkedItemAsync(int itemId);
    Task<bool> HasActiveItemAsync(int itemId);
}
