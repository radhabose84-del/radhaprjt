namespace Contracts.Interfaces.Validations.SalesManagement;

public interface ISalesUomValidation
{
    Task<bool> HasLinkedUomAsync(int uomId);
    Task<bool> HasActiveUomAsync(int uomId);
}
