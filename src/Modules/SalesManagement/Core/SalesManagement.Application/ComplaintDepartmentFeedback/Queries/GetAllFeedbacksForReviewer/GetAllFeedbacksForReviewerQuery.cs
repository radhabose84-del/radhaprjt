using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedbacksForReviewer
{
    /// <summary>
    /// Reviewer-scoped list of department feedbacks. Unlike GetAllComplaintDepartmentFeedbackQuery
    /// (which scopes to the logged-in user's own assignments), this query returns feedbacks
    /// across ALL responsible persons — used by QC reviewers and supervisors to scan
    /// every department's submission. Same DTO shape, same search/status filters.
    /// Authorization (which roles may call this) is gated outside this handler.
    /// </summary>
    public class GetAllFeedbacksForReviewerQuery : IRequest<ApiResponseDTO<List<FeedbackListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }
}
