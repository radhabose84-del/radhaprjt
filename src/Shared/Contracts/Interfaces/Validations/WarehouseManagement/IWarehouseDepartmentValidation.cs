namespace Contracts.Interfaces.Validations.WarehouseManagement;

public interface IWarehouseDepartmentValidation
{
    /// <summary>Checks if any non-deleted Warehouse record references this department (for delete guard).</summary>
    Task<bool> HasLinkedDepartmentAsync(int departmentId);

    /// <summary>Checks if any active Warehouse record references this department (for inactivate guard).</summary>
    Task<bool> HasActiveDepartmentAsync(int departmentId);
}
