namespace Contracts.Interfaces.Validations.ProjectManagement;

public interface IProjectCurrencyValidation
{
    Task<bool> HasLinkedCurrencyAsync(int currencyId);
}
