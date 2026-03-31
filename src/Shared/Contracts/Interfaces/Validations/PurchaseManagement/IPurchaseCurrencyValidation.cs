namespace Contracts.Interfaces.Validations.PurchaseManagement;

public interface IPurchaseCurrencyValidation
{
    Task<bool> HasLinkedCurrencyAsync(int currencyId);
}
