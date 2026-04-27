using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IComplaint
{
    public interface IComplaintCommandRepository
    {
        Task<int> CreateAsync(ComplaintHeader entity, int typeId);
        Task<int> UpdateAsync(ComplaintHeader entity, List<ComplaintDetail> details);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task UpdateApprovalStatusAsync(int id, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct);
        Task UpdateQCReviewApprovalStatusAsync(int complaintHeaderId, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct);
        Task UpdateResolutionApprovalStatusAsync(int complaintHeaderId, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct);
        Task EnsureResolutionDraftIfQCAcceptedAsync(int complaintHeaderId, int createdBy, string? createdByName, string? createdIP, CancellationToken ct);
        Task<int> AddAttachmentAsync(ComplaintAttachment attachment);
        Task<bool> DeleteAttachmentAsync(int id, CancellationToken ct);
    }
}
