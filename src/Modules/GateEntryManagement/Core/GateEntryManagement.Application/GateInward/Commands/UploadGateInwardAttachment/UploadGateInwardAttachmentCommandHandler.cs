using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment
{
    public class UploadGateInwardAttachmentCommandHandler
        : IRequestHandler<UploadGateInwardAttachmentCommand, UploadGateInwardAttachmentResultDto>
    {
        private readonly IGateInwardAttachmentFileStorage _storage;

        public UploadGateInwardAttachmentCommandHandler(IGateInwardAttachmentFileStorage storage)
        {
            _storage = storage;
        }

        public async Task<UploadGateInwardAttachmentResultDto> Handle(
            UploadGateInwardAttachmentCommand request, CancellationToken ct)
        {
            var staged = await _storage.SaveToStagingAsync(request.File!, ct);

            return new UploadGateInwardAttachmentResultDto(FileName: staged.FileName);
        }
    }
}
