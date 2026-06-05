using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.UploadDocument;

public class UploadRawMaterialPODocumentCommandHandler
    : IRequestHandler<UploadRawMaterialPODocumentCommand, UploadRawMaterialPODocumentResultDto>
{
    private readonly IRawMaterialPOFileStorage _storage;

    public UploadRawMaterialPODocumentCommandHandler(IRawMaterialPOFileStorage storage)
    {
        _storage = storage;
    }

    public async Task<UploadRawMaterialPODocumentResultDto> Handle(UploadRawMaterialPODocumentCommand request, CancellationToken ct)
    {
        var saved = await _storage.SaveAsync(request.File!, ct);

        return new UploadRawMaterialPODocumentResultDto(
            FileName: saved.FileName,
            OriginalFileName: saved.OriginalFileName,
            FileSize: saved.FileSize,
            FileType: saved.FileType);
    }
}
