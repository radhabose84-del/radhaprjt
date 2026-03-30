namespace SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback
{
    public interface IComplaintDepartmentFeedbackCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ComplaintDepartmentFeedback entity);
        Task<int> UpdateAsync(Domain.Entities.ComplaintDepartmentFeedback entity, ICollection<Domain.Entities.ComplaintFeedbackAttachment>? attachments);
        Task<int> UpdateStatusAsync(int id, int feedbackStatusId, string? reworkReason, int reworkCount);
        Task<int> UpdateAssignmentStatusAsync(int assignmentId, int assignmentStatusId);
    }
}
