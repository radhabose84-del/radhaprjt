using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.UploadGateEntryDocument
{
    public class UploadGateEntryDocumentCommand : IRequest<GateEntryDocumentDto>
    {
         public IFormFile? File { get; set; }  
    }
}