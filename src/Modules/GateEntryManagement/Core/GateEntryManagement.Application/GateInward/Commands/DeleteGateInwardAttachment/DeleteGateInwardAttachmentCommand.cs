using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.DeleteGateInwardAttachment
{
    public sealed record DeleteGateInwardAttachmentCommand(int GateInwardHdrId) : IRequest<bool>;
}
