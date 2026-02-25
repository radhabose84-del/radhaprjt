using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument
{
    public class DeleteGRNEntryDocumentCommand : IRequest<bool>
    {
        //Local Folder Delete
        public string? GrnEntrydocumentPath { get; set; }
    }
}