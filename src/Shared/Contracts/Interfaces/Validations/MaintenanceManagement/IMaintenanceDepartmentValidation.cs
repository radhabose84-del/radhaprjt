namespace Contracts.Interfaces.Validations.MaintenanceManagement;

public interface IMaintenanceDepartmentValidation
{
    /// <summary>Checks if any non-deleted Maintenance record references this department (for delete guard).</summary>
    Task<bool> HasLinkedDepartmentAsync(int departmentId);

    /// <summary>Checks if any active Maintenance record references this department (for inactivate guard).</summary>
    Task<bool> HasActiveDepartmentAsync(int departmentId);
}
