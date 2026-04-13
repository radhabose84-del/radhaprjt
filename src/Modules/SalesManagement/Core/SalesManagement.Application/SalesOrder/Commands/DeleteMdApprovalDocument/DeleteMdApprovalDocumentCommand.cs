using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteMdApprovalDocument
{
    public class DeleteMdApprovalDocumentCommand : IRequest<bool>
    {
        public string? FilePath { get; set; }
    }
}
