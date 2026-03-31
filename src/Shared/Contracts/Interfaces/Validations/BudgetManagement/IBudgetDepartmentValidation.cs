namespace Contracts.Interfaces.Validations.BudgetManagement;

public interface IBudgetDepartmentValidation
{
    /// <summary>Checks if any non-deleted Budget record references this department (for delete guard).</summary>
    Task<bool> HasLinkedDepartmentAsync(int departmentId);

    /// <summary>Checks if any active Budget record references this department (for inactivate guard).</summary>
    Task<bool> HasActiveDepartmentAsync(int departmentId);
}
