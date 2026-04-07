namespace Contracts.Interfaces.Validations.SalesManagement;

public interface ISalesHsnValidation
{
    Task<bool> HasLinkedHsnAsync(int hsnId);
    Task<bool> HasActiveHsnAsync(int hsnId);
}
