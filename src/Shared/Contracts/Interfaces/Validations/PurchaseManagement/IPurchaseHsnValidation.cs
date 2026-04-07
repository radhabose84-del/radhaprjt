namespace Contracts.Interfaces.Validations.PurchaseManagement;

public interface IPurchaseHsnValidation
{
    Task<bool> HasLinkedHsnAsync(int hsnId);
    Task<bool> HasActiveHsnAsync(int hsnId);
}
