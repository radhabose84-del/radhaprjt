using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback
{
    public class UpdateComplaintDepartmentFeedbackCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? RootCauseText { get; set; }
        public int? RootCauseCategoryId { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<SubmitAttachmentDto>? Attachments { get; set; }
    }
}
