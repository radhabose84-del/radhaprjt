namespace Contracts.Interfaces.Lookups.Users;

/// <summary>
/// Resolves the active application users who belong to the department(s) mapped
/// (via <c>AppData.ApprovalStepDepartmentMapping</c>) to an approval step,
/// identified by the step's <c>TargetType.Code</c> — e.g.
/// <c>COMPLAINT_QC_REVIEWER_USER</c>.
///
/// This mirrors <c>AppData.sp_EvaluateApproval</c> Block 4. It exists so that
/// InApp notifications targeting the QC team can be routed dynamically in C#
/// (via <c>NotificationCreatedEvent.OverrideTargetUserIds</c>) instead of the
/// static <c>dbo.WorkFlow_GetUserId</c> / <c>NotificationLevelHierarchy</c> seed,
/// which cannot resolve the department team per workflow.
/// </summary>
public interface IDepartmentUserLookup
{
    /// <summary>
    /// Returns the distinct active <c>AppSecurity.Users.UserId</c> values whose
    /// <c>DepartmentId</c> matches a department mapped to the approval step whose
    /// <c>TargetType.Code</c> equals <paramref name="targetTypeCode"/>
    /// (case-insensitive, trimmed). Returns an empty list when nothing resolves
    /// — callers fall back to the configured dispatcher recipient.
    /// </summary>
    Task<IReadOnlyList<int>> GetActiveUserIdsByApprovalStepTargetTypeAsync(
        string targetTypeCode, CancellationToken ct = default);
}
