namespace Contracts.Interfaces.Validations.PurchaseManagement;

public interface IPurchaseUomValidation
{
    Task<bool> HasLinkedUomAsync(int uomId);
    Task<bool> HasActiveUomAsync(int uomId);
}
