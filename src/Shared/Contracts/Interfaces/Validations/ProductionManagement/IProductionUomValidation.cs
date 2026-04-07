namespace Contracts.Interfaces.Validations.ProductionManagement;

public interface IProductionUomValidation
{
    Task<bool> HasLinkedUomAsync(int uomId);
    Task<bool> HasActiveUomAsync(int uomId);
}
