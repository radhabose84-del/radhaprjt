using MediatR;
using Microsoft.AspNetCore.Http;

namespace ProjectManagement.Application.UploadDocument
{
    public class UploadDocumentCommand : IRequest<DocumentDto>
    {
        public IFormFile? File { get; set; }  
        
    }
}