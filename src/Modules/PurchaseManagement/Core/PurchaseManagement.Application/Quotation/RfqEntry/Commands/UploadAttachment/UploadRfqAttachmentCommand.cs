using MediatR;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.UploadAttachment;

public class UploadRfqAttachmentCommand : IRequest<UploadRfqAttachmentResultDto>
{
    public IFormFile? File { get; set; }
}
