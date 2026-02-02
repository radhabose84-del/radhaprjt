using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument
{
    public class DeleteGRNEntryDocumentCommand : IRequest<bool>
    {
        //Local Folder Delete
        public string? GrnEntrydocumentPath { get; set; }
    }
}