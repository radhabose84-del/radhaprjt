namespace Contracts.Interfaces.Validations.FinanceManagement;

public interface IPartyMasterFinanceValidation
{
    Task<bool> HasLinkedPartyMasterAsync(int partyId);
    Task<bool> HasActivePartyMasterAsync(int partyId);
}
