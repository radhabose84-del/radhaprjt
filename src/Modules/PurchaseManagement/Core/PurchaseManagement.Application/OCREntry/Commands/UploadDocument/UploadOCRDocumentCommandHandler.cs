using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Commands.UploadDocument;

public class UploadOCRDocumentCommandHandler : IRequestHandler<UploadOCRDocumentCommand, UploadOCRDocumentResultDto>
{
    private readonly IOCREntryFileStorage _storage;

    public UploadOCRDocumentCommandHandler(IOCREntryFileStorage storage)
    {
        _storage = storage;
    }

    public async Task<UploadOCRDocumentResultDto> Handle(UploadOCRDocumentCommand request, CancellationToken ct)
    {
        var saved = await _storage.SaveAsync(request.File!, ct);

        return new UploadOCRDocumentResultDto(
            FileName: saved.FileName,
            OriginalFileName: saved.OriginalFileName,
            FileSize: saved.FileSize,
            FileType: saved.FileType);
    }
}
