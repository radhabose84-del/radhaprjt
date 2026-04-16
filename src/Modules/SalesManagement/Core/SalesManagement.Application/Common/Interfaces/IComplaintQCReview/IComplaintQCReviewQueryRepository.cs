using SalesManagement.Application.ComplaintQCReview.Dto;

namespace SalesManagement.Application.Common.Interfaces.IComplaintQCReview
{
    public interface IComplaintQCReviewQueryRepository
    {
        Task<(List<QCReviewListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter);
        Task<ComplaintQCReviewDto?> GetByIdAsync(int id);
        Task<ComplaintQCReviewDto?> GetByComplaintIdAsync(int complaintHeaderId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ComplaintExistsAsync(int complaintHeaderId);
        Task<bool> IsComplaintApprovedAsync(int complaintHeaderId);
        Task<bool> ReviewAlreadyExistsAsync(int complaintHeaderId, int? excludeId = null);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> UserExistsAsync(int userId);
    }
}
