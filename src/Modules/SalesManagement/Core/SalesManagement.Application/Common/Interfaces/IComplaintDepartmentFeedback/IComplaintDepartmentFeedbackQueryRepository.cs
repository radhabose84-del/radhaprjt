using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback
{
    public interface IComplaintDepartmentFeedbackQueryRepository
    {
        Task<(List<FeedbackListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter, int? responsiblePersonId = null);
        Task<ComplaintDepartmentFeedbackDto?> GetByIdAsync(int id);
        Task<ComplaintDepartmentFeedbackDto?> GetByAssignmentIdAsync(int assignmentId);
        Task<List<FeedbackListDto>> GetByComplaintIdAsync(int complaintHeaderId);

        /// <summary>
        /// Per-complaint detail view for QC: returns one row per assigned department with
        /// full RCA content + attachments. FeedbackId is null when a dept hasn't submitted yet.
        /// </summary>
        Task<List<ComplaintFeedbackFullDto>> GetByComplaintIdWithContentAsync(int complaintHeaderId);

        Task<List<MyPendingFeedbackDto>> GetMyPendingAsync(int userId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AssignmentExistsAsync(int assignmentId);
        Task<bool> IsQCApprovedForAssignmentAsync(int assignmentId);
        Task<bool> IsQCApprovedForFeedbackAsync(int feedbackId);
        Task<bool> FeedbackAlreadyExistsForAssignmentAsync(int assignmentId);
        Task<int> GetResponsiblePersonIdAsync(int assignmentId);
        Task<int> GetFeedbackStatusIdAsync(int feedbackId);
        Task<int> GetAssignmentIdByFeedbackIdAsync(int feedbackId);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<(int ReworkCount, int FeedbackStatusId)> GetReworkInfoAsync(int feedbackId);
        Task<string?> GetAttachmentFilePathAsync(int id);
    }
}
