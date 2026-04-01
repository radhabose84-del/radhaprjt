namespace Contracts.Interfaces.Validations.MaintenanceManagement;

public interface IMaintenanceManufacturerValidation
{
    /// <summary>Checks if any non-deleted Maintenance record references this manufacturer (for delete guard).</summary>
    Task<bool> HasLinkedManufacturerAsync(int manufacturerId);

    /// <summary>Checks if any active Maintenance record references this manufacturer (for inactivate guard).</summary>
    Task<bool> HasActiveManufacturerAsync(int manufacturerId);
}
