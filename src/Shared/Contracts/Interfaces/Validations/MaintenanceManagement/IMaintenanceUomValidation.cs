namespace Contracts.Interfaces.Validations.MaintenanceManagement;

public interface IMaintenanceUomValidation
{
    /// <summary>Checks if any non-deleted Maintenance record references this UOM (for delete guard).</summary>
    Task<bool> HasLinkedUomAsync(int uomId);

    /// <summary>Checks if any active Maintenance record references this UOM (for inactivate guard).</summary>
    Task<bool> HasActiveUomAsync(int uomId);
}
