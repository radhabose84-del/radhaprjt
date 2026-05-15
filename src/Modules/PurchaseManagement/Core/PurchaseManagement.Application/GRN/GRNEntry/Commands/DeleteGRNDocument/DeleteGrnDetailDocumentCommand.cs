using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument
{
    public class DeleteGrnDetailDocumentCommand : IRequest<bool>
    {
        //Local Folder Delete
        public string? GrnDetaildocumentPath { get; set; }
    }
}
