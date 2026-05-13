using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.DeleteAttachment;

public sealed record DeleteRfqAttachmentCommand(int RfqId, int AttachmentId) : IRequest<bool>;
