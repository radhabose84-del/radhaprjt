namespace Contracts.Interfaces.Validations.PurchaseManagement;

public interface IPartyMasterPurchaseValidation
{
    Task<bool> HasLinkedPartyMasterAsync(int partyId);
    Task<bool> HasActivePartyMasterAsync(int partyId);
}
