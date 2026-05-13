using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.SalesAgreement.Commands.UploadSalesAgreementDocument
{
    public class UploadSalesAgreementDocumentCommand : IRequest<SalesAgreementDocumentDto>
    {
        public IFormFile? File { get; set; }
    }
}
