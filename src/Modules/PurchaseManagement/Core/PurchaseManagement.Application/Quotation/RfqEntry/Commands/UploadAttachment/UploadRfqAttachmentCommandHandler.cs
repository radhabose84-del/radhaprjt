using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.UploadAttachment;

public class UploadRfqAttachmentCommandHandler : IRequestHandler<UploadRfqAttachmentCommand, UploadRfqAttachmentResultDto>
{
    private readonly IRfqAttachmentFileStorage _storage;

    public UploadRfqAttachmentCommandHandler(IRfqAttachmentFileStorage storage)
    {
        _storage = storage;
    }

    public async Task<UploadRfqAttachmentResultDto> Handle(UploadRfqAttachmentCommand request, CancellationToken ct)
    {
        var staged = await _storage.SaveToStagingAsync(request.File!, ct);

        return new UploadRfqAttachmentResultDto(
            FileName: staged.FileName,
            OriginalFileName: staged.OriginalFileName,
            FileSize: staged.FileSize,
            FileType: staged.FileType);
    }
}
