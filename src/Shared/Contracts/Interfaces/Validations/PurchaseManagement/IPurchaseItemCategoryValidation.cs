namespace Contracts.Interfaces.Validations.PurchaseManagement;

public interface IPurchaseItemCategoryValidation
{
    Task<bool> HasLinkedItemCategoryAsync(int itemCategoryId);
    Task<bool> HasActiveItemCategoryAsync(int itemCategoryId);
}
