using MediatR;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision;

/// <summary>
/// Called by the BackgroundService approval pipeline (via MassTransit consumer) when
/// an approval decision is reached for a Purchase Return transaction.
/// </summary>
public sealed record ProcessPurchaseReturnApprovalDecisionCommand(
    int PurchaseReturnHeaderId,
    bool IsApproved,
    int? ApprovalRequestId,
    string? ApproverRemarks
) : IRequest<bool>;
