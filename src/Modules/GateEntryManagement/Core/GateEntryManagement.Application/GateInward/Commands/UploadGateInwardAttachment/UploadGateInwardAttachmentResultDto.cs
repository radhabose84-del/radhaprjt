namespace GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment
{
    public sealed record UploadGateInwardAttachmentResultDto(
        string FileName,
        string OriginalFileName,
        long FileSize,
        string? FileType);
}
