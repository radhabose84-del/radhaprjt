namespace Contracts.Interfaces.Validations.SalesManagement;

public interface IPackTypeSalesValidation
{
    Task<bool> HasLinkedPackTypeAsync(int packTypeId);
    Task<bool> HasActivePackTypeAsync(int packTypeId);
}
