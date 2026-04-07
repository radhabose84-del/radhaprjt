namespace Contracts.Interfaces.Validations.SalesManagement;

public interface IPartyMasterSalesValidation
{
    Task<bool> HasLinkedPartyMasterAsync(int partyId);
    Task<bool> HasActivePartyMasterAsync(int partyId);
}
