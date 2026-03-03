using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderImage
{
    public class UploadSalesOrderImageCommand : IRequest<SalesOrderDocumentDto>
    {
        public IFormFile? File { get; set; }
    }
}
