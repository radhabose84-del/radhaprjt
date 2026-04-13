using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.UploadMdApprovalDocument
{
    public class UploadMdApprovalDocumentCommand : IRequest<SalesOrderDocumentDto>
    {
        public IFormFile? File { get; set; }
    }
}
