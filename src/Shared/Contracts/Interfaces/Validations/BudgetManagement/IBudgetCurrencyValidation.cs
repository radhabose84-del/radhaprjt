namespace Contracts.Interfaces.Validations.BudgetManagement;

public interface IBudgetCurrencyValidation
{
    Task<bool> HasLinkedCurrencyAsync(int currencyId);
}
