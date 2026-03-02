using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderDocument
{
    public class UploadSalesOrderDocumentCommand : IRequest<SalesOrderDocumentDto>
    {
        public IFormFile? File { get; set; }
        public string? DocumentType { get; set; }
    }
}
