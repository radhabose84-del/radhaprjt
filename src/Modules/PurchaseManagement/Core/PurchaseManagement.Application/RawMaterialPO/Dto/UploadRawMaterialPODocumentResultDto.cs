namespace PurchaseManagement.Application.RawMaterialPO.Dto;

// FileName is the value the client must send back as Create/UpdateRawMaterialPOCommand.DocumentPath.
// On create/update the file is renamed to "{PONumber}{ext}".
public sealed record UploadRawMaterialPODocumentResultDto(
    string FileName,
    string OriginalFileName,
    long FileSize,
    string? FileType);
