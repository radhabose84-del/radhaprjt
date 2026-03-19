namespace Contracts.Interfaces;

/// <summary>
/// Provides runtime data access filtering context for the current authenticated user.
/// Repositories inject this to restrict query results based on role-based data access rules.
/// </summary>
public interface IDataAccessFilter
{
    /// <summary>
    /// Returns the data access context for the current user.
    /// Result is cached for the duration of the HTTP request.
    /// </summary>
    Task<DataAccessContext> GetContextAsync(CancellationToken ct = default);
}
