using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IComplaint
{
    public interface IComplaintCommandRepository
    {
        Task<int> CreateAsync(ComplaintHeader entity);
        Task<int> UpdateAsync(ComplaintHeader entity, List<ComplaintDetail> details);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct);
    }
}
