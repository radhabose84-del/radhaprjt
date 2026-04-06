using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Commands.UploadAttachment
{
    public class UploadComplaintAttachmentCommand : IRequest<ComplaintAttachmentDto>
    {
        public int ComplaintHeaderId { get; set; }
        public IFormFile? File { get; set; }
    }
}
