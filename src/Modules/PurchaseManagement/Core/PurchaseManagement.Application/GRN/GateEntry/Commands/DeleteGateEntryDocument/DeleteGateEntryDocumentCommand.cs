using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.DeleteGateEntryDocument
{
    public class DeleteGateEntryDocumentCommand : IRequest<bool>
    {
        //Local Folder Delete
        public string? GateEntrydocumentPath { get; set; }
    }
}