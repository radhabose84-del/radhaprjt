namespace Contracts.Interfaces.Validations.MaintenanceManagement;

public interface IPartyMasterMaintenanceValidation
{
    Task<bool> HasLinkedPartyMasterAsync(int partyId);
    Task<bool> HasActivePartyMasterAsync(int partyId);
}
