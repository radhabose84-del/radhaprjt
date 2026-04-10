using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IComplaint
{
    public interface IComplaintCommandRepository
    {
        Task<int> CreateAsync(ComplaintHeader entity, int typeId);
        Task<int> UpdateAsync(ComplaintHeader entity, List<ComplaintDetail> details);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct);
        Task UpdateQCReviewApprovalStatusAsync(int complaintHeaderId, string status, CancellationToken ct);
        Task UpdateResolutionApprovalStatusAsync(int complaintHeaderId, string status, int modifiedBy, CancellationToken ct);
        Task<int> AddAttachmentAsync(ComplaintAttachment attachment);
        Task<bool> DeleteAttachmentAsync(int id, CancellationToken ct);
    }
}
