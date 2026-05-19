using MediatR;
using Microsoft.AspNetCore.Http;

namespace GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment
{
    public class UploadGateInwardAttachmentCommand : IRequest<UploadGateInwardAttachmentResultDto>
    {
        public IFormFile? File { get; set; }
    }
}
