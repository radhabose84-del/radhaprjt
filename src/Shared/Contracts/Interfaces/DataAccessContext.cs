namespace Contracts.Interfaces;

/// <summary>
/// Encapsulates all data access filtering state for the current user.
/// Consumed by query repositories to append WHERE-clause conditions.
/// </summary>
public sealed class DataAccessContext
{
    /// <summary>
    /// When true, skip all data access filtering. User has unrestricted access.
    /// True when ANY of the user's roles has BypassDataAccess = true.
    /// </summary>
    public bool BypassDataAccess { get; init; }

    /// <summary>
    /// The user's PartyId from the User table (nullable).
    /// If set, identifies the user as an Agent or Marketing Officer in PartyManagement.
    /// </summary>
    public int? PartyId { get; init; }

    /// <summary>
    /// Union of all ItemGroupIds across all of the user's active roles.
    /// Empty set means no items are accessible (unless BypassDataAccess is true).
    /// </summary>
    public IReadOnlySet<int> AllowedItemGroupIds { get; init; } = new HashSet<int>();

    /// <summary>
    /// CustomerIds accessible to this user via AgentCustomerMapping.
    /// Empty set means no customer filtering (unless BypassDataAccess is true).
    /// </summary>
    public IReadOnlySet<int> AllowedCustomerIds { get; init; } = new HashSet<int>();

    /// <summary>
    /// AgentIds accessible to this user (for Officer-level access via OfficerAgent).
    /// Empty set means no agent filtering.
    /// </summary>
    public IReadOnlySet<int> AllowedAgentIds { get; init; } = new HashSet<int>();

    /// <summary>
    /// True when the user is subject to customer-level filtering (Agent with PartyId, or Marketing Officer with EmpId).
    /// Repositories should use this instead of PartyId.HasValue to decide whether to apply customer filters.
    /// </summary>
    public bool IsCustomerRestricted { get; init; }

    /// <summary>
    /// Static instance representing "no filtering" (bypass mode or unauthenticated).
    /// </summary>
    public static DataAccessContext Unrestricted { get; } = new()
    {
        BypassDataAccess = true
    };
}
