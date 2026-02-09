using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ProjectManagement.Application.UploadDocument
{
    public class UploadDocumentCommand : IRequest<DocumentDto>
    {
        public IFormFile? File { get; set; }  
        
    }
}