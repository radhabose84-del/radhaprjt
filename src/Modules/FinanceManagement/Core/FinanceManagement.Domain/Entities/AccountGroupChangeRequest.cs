using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // A pending Move ("hierarchy restructure") awaiting Finance Controller approval.
    // Raised by MoveAccountGroupCommandHandler; applied by the ApprovedRejectedConsumer on approval.
    public class AccountGroupChangeRequest : BaseEntity
    {
        public int AccountGroupId { get; set; }
        public int NewParentAccountGroupId { get; set; }
        public string Justification { get; set; } = null!;
        public int ApproverId { get; set; }

        // Pending / Approved / Rejected (named RequestStatus to avoid hiding BaseEntity.Status enum).
        public string RequestStatus { get; set; } = "Pending";
    }
}
