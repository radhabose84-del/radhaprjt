namespace Contracts.Interfaces.Validations.SalesManagement;

public interface ILotMasterSalesValidation
{
    Task<bool> HasLinkedLotMasterAsync(int lotMasterId);
    Task<bool> HasActiveLotMasterAsync(int lotMasterId);
}
