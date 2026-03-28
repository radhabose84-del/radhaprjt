using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UploadAttachment
{
    public class UploadFeedbackAttachmentCommand : IRequest<FeedbackAttachmentUploadDto>
    {
        public IFormFile? File { get; set; }
    }
}
