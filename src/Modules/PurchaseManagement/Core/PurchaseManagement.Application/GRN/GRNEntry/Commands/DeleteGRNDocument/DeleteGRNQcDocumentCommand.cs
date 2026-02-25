using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument
{
    public class DeleteGRNQcDocumentCommand : IRequest<bool>
    {
        //Local Folder Delete
        public string? GrnQcdocumentPath { get; set; }
    }
}