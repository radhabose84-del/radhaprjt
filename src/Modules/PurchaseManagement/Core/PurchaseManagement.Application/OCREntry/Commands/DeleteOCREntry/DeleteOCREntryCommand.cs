using MediatR;

namespace PurchaseManagement.Application.OCREntry.Commands.DeleteOCREntry
{
    public sealed record DeleteOCREntryCommand(int Id) : IRequest<bool>;
}
