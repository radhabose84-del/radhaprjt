using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument
{
    public class UploadGrnQcDocumentCommand : IRequest<GRNQcImageDto>
    {
         public IFormFile? File { get; set; }  
    }
}