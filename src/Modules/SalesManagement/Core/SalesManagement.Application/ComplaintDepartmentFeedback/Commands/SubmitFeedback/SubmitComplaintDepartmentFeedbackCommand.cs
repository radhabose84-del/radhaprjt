using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback
{
    public class SubmitComplaintDepartmentFeedbackCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int AssignmentId { get; set; }
        public string? RootCauseText { get; set; }
        public int? RootCauseCategoryId { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? Remarks { get; set; }
        public List<SubmitAttachmentDto>? Attachments { get; set; }
    }
}
