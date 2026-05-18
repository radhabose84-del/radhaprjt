using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent
{
    public class UpdatePurchaseIndentCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public string Purpose { get; set; } = default!;
        public int DepartmentId { get; set; }
        public byte IsActive { get; set; }
        public byte IsDraft { get; set; }
        // true ONLY when the save originates from the approver's edit-during-approval screen.
        // Drives the one-time OldQuantityRequired capture. Normal creator edits send false.
        public bool IsApprovalEdit { get; set; }
        public ICollection<IndentDetailUpdateDto> IndentDetails { get; set; } = default!;
    }
}