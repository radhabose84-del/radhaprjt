using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback
{
    public interface IComplaintDepartmentFeedbackQueryRepository
    {
        Task<(List<FeedbackListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ComplaintDepartmentFeedbackDto?> GetByIdAsync(int id);
        Task<ComplaintDepartmentFeedbackDto?> GetByAssignmentIdAsync(int assignmentId);
        Task<List<FeedbackListDto>> GetByComplaintIdAsync(int complaintHeaderId);
        Task<List<MyPendingFeedbackDto>> GetMyPendingAsync(int userId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AssignmentExistsAsync(int assignmentId);
        Task<bool> FeedbackAlreadyExistsForAssignmentAsync(int assignmentId);
        Task<int> GetResponsiblePersonIdAsync(int assignmentId);
        Task<int> GetFeedbackStatusIdAsync(int feedbackId);
        Task<int> GetAssignmentIdByFeedbackIdAsync(int feedbackId);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<(int ReworkCount, int FeedbackStatusId)> GetReworkInfoAsync(int feedbackId);
    }
}
