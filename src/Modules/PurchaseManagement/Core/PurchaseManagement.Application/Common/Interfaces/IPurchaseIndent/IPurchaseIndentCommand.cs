using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent
{
    public interface IPurchaseIndentCommand
    {
        Task<IndentHeader> CreateAsync(IndentHeader indentHeader);
        // Returns the updated EF-tracked entity on success (so the handler can build the approval
        // payload from it without a second round-trip via Dapper — that round-trip would deadlock
        // on the row locks held by this method's open transaction). Returns null when the indent
        // does not exist.
        Task<IndentHeader?> UpdateAsync(IndentHeader indentHeader, string request, bool isApprovalEdit = false);
        Task<bool> DeleteAsync(int id, IndentHeader indentHeader);
        // Task<List<IndentDetail>> UpdateIndentDetailAsync(List<IndentDetail> indentDetail);
        Task<bool> RollbackStatusAsync(int id);
        Task<bool> FinalizeStatus(IndentHeader indentHeader);
        Task<bool> UpdateRFQStatusAsync(IEnumerable<int> indentDetailIds);
    }
}