namespace Contracts.Interfaces.Validations.ProjectManagement;

public interface IProjectDepartmentValidation
{
    /// <summary>Checks if any non-deleted Project record references this department (for delete guard).</summary>
    Task<bool> HasLinkedDepartmentAsync(int departmentId);

    /// <summary>Checks if any active Project record references this department (for inactivate guard).</summary>
    Task<bool> HasActiveDepartmentAsync(int departmentId);
}
