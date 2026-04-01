namespace Contracts.Interfaces.Validations.SalesManagement;

public interface ISalesCurrencyValidation
{
    Task<bool> HasLinkedCurrencyAsync(int currencyId);
}
